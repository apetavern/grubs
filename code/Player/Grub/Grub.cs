using System.ComponentModel;
using System.Threading.Tasks;
using Grubs.Crates;
using Grubs.States;
using Grubs.Terrain;
using Grubs.Utils;
using Grubs.Utils.Event;
using Grubs.Weapons.Base;

namespace Grubs.Player;

/// <summary>
/// A playable grub.
/// </summary>
[Category( "Grubs" )]
public sealed partial class Grub : AnimatedEntity, IDamageable, IResolvable
{
	/// <summary>
	/// The grubs movement controller.
	/// </summary>
	[Net, Predicted]
	public GrubController Controller { get; private set; } = null!;

	/// <summary>
	/// The currently active weapon the grub is using.
	/// </summary>
	[Net]
	public GrubWeapon? ActiveChild { get; private set; }

	/// <summary>
	/// The last weapon the grub was using.
	/// </summary>
	[Net]
	public GrubWeapon? LastActiveChild { get; private set; }

	/// <summary>
	/// The grubs gravestone.
	/// </summary>
	[Net]
	public Gravestone? Gravestone { get; private set; }

	/// <summary>
	/// Helper property to get a grubs team.
	/// </summary>
	public Team Team => (Owner as Team)!;

	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	/// <summary>
	/// Returns whether or not this grub has been damaged.
	/// </summary>
	[Net]
	public bool HasBeenDamaged { get; private set; }

	private readonly Queue<DamageInfo> _damageQueue = new();
	private bool _takeDamage;

	/// <summary>
	/// Returns whether it is this grubs turn.
	/// </summary>
	public bool IsTurn
	{
		get
		{
			if ( Owner is not Team team )
				return false;

			return team.ActiveGrub == this && team.IsTurn;
		}
	}

	/// <summary>
	/// Returns whether or not this grub is resolved and ready to move to the next turn.
	/// </summary>
	public bool Resolved => Velocity.IsNearlyZero( 0.1f ) || LifeState == LifeState.Dead;

	/// <summary>
	/// Whether or not this grub is facing left.
	/// </summary>
	public bool FacingLeft => Rotation.z < 0;

	/// <summary>
	/// Whether or not this grub is facing right.
	/// </summary>
	public bool FacingRight => !FacingLeft;

	/// <summary>
	/// The reason for this grubs death.
	/// <remarks>This will only be available on the server.</remarks>
	/// </summary>
	public GrubDeathReason? DeathReason;

	/// <summary>
	/// The running task to kill the grub.
	/// <remarks>This will only be available on the server.</remarks>
	/// </summary>
	public Task? DeathTask;

	private const float MaxHealth = 100;
	private const float MaxOverhealHealth = 250;

	public Grub()
	{
		Transmit = TransmitType.Always;
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( !Game.IsServer )
			return;

		if ( !_takeDamage )
		{
			if ( IsTurn && BaseState.Instance is BaseGamemode gamemode )
				gamemode.UseTurn( false );

			_damageQueue.Enqueue( info );
			HasBeenDamaged = true;
			return;
		}

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;

		if ( Health <= 0 || LifeState != LifeState.Alive )
			return;

		Health -= info.Damage;
		EventRunner.RunLocal( GrubsEvent.GrubHurtEvent, this, info.Damage );
		HurtRpc( To.Everyone, info.Damage );

		if ( Health > 0 )
			return;

		Health = 0;
		OnKilled();
	}

	public override void OnKilled()
	{
		if ( LifeState is LifeState.Dying or LifeState.Dead )
			return;

		DeathTask = Die();
	}

	public void Spawn( IClient? cl = null )
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Name = Game.Random.FromArray( GameConfig.GrubNames );
		Health = MaxHealth;
		EnableHitboxes = true;

		Controller = new GrubController();

		if ( cl is not null )
			DressFromClient( cl );
		SetHatVisible( true );
	}

	private async Task Die()
	{
		if ( DeathReason!.Value.FromKillTrigger )
		{
			FinishDie();
			return;
		}

		await GameTask.Delay( 200 );
		LifeState = LifeState.Dying;
		var plunger = new ModelEntity( "models/tools/dynamiteplunger/dynamiteplunger.vmdl" )
		{
			Position = FacingLeft ? Position - new Vector3( 30, 0, 0 ) : Position
		};
		await GameTask.Delay( 1025 );

		ExplosionHelper.Explode( Position, this, 50 );
		plunger.Delete();
		FinishDie();
	}

	private void FinishDie()
	{
		LifeState = LifeState.Dead;
		// TODO: Hide the grub instead of deleting it. Possible revive mechanic?
		EnableDrawing = false;
		Gravestone = new Gravestone( this ) { Owner = this, Parent = this };

		ChatBox.AddInformation( To.Everyone, DeathReason.ToString(), $"avatar:{Team.ActiveClient.SteamId}" );
		EventRunner.RunLocal( GrubsEvent.GrubDiedEvent, this );
		DeadRpc( To.Everyone );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Gravestone?.Simulate( cl );
		Controller?.Simulate( cl, this );

		if ( LifeState != LifeState.Dead )
		{
			foreach ( var zone in TerrainZone.All.OfType<PickupZone>() )
			{
				if ( !zone.IsValid )
					continue;

				if ( zone.InZone( this ) )
					zone.Trigger( this );
			}
		}

		SimulateActiveChild( cl, ActiveChild );
	}

	/// <summary>
	/// Simulates the active weapon.
	/// </summary>
	/// <param name="client">The client that is predicting.</param>
	/// <param name="child">The weapon to simulate.</param>
	private void SimulateActiveChild( IClient client, GrubWeapon? child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
			return;

		LastActiveChild.Simulate( client );
	}

	/// <summary>
	/// Called when the active weapon has changed.
	/// </summary>
	/// <param name="previous">The old weapon that was being used.</param>
	/// <param name="next">The new weapon being used.</param>
	private void OnActiveChildChanged( GrubWeapon? previous, GrubWeapon? next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	/// <summary>
	/// Attempts to heal the Grub by the given amount.
	/// </summary>
	/// <param name="health">The amount of health to try giving to the Grub.</param>
	/// <returns>Whether or not any healing was applied.</returns>
	public bool GiveHealth( float health )
	{
		Game.AssertServer();

		var healthToGive = Math.Min( health, MaxOverhealHealth - Health );
		if ( healthToGive <= 0 )
			return false;

		Health += healthToGive;
		EventRunner.RunLocal( GrubsEvent.GrubHealedEvent, this, health );
		HealRpc( To.Everyone, healthToGive );
		return true;
	}

	/// <summary>
	/// Applies any damage that this grub has received.
	/// </summary>
	/// <returns>Whether or not the grub has died.</returns>
	public bool ApplyDamage()
	{
		Game.AssertServer();

		if ( !HasBeenDamaged )
			return false;

		_takeDamage = true;

		var totalDamage = 0f;
		var damageInfos = new List<DamageInfo>();
		while ( _damageQueue.TryDequeue( out var damageInfo ) )
		{
			damageInfos.Add( damageInfo );
			totalDamage += damageInfo.Damage;
		}

		var dead = false;
		if ( totalDamage >= Health )
		{
			dead = true;
			DeathReason = GrubDeathReason.FindReason( this, damageInfos );
		}

		TakeDamage( DamageInfo.Generic( Math.Min( totalDamage, Health ) ) );

		_takeDamage = false;
		HasBeenDamaged = false;
		return dead;
	}

	/// <summary>
	/// Equips a new weapon for this grub.
	/// </summary>
	/// <param name="weapon">The weapon to equip.</param>
	public void EquipWeapon( GrubWeapon? weapon )
	{
		Game.AssertServer();

		ActiveChild = weapon;
	}

	/// <summary>
	/// Dresses the grub based on the provided clients avatar.
	/// </summary>
	/// <param name="cl">The client to get the clothes of.</param>
	private void DressFromClient( IClient cl )
	{
		var clothes = new ClothingContainer();
		clothes.LoadFromClient( cl );

		// Skin tone
		var skinTone = clothes.Clothing.FirstOrDefault( model => model.Model == "models/citizenworm.vmdl" );
		SetMaterialGroup( skinTone?.MaterialGroup );

		// We only want the hair/hats so we won't use the logic built into Clothing
		var items = clothes.Clothing.Where( item =>
			item.Category is Clothing.ClothingCategory.Hair or Clothing.ClothingCategory.Hat
				or Clothing.ClothingCategory.Facial or Clothing.ClothingCategory.Skin
		);

		foreach ( var item in items )
		{
			var ent = new AnimatedEntity( item.Model, this );

			// Add a tag to the hat so we can reference it later.
			if ( item.Category is Clothing.ClothingCategory.Hat or Clothing.ClothingCategory.Hair )
				ent.Tags.Add( "head" );

			if ( !string.IsNullOrEmpty( item.MaterialGroup ) )
				ent.SetMaterialGroup( item.MaterialGroup );

			if ( item.Category != Clothing.ClothingCategory.Skin )
				continue;

			var skinMaterial = Material.Load( item.SkinMaterial );
			SetMaterialOverride( skinMaterial, "skin" );
		}
	}

	/// <summary>
	/// Sets whether the hat should be visible on the grub.
	/// </summary>
	/// <param name="visible"></param>
	public void SetHatVisible( bool visible )
	{
		var hats = Children.OfType<AnimatedEntity>().Where( child => child.Tags.Has( "head" ) );

		foreach ( var hat in hats )
			hat.EnableDrawing = visible;
	}

	[ClientRpc]
	private void HealRpc( float health )
	{
		EventRunner.RunLocal( GrubsEvent.GrubHealedEvent, this, health );
	}

	[ClientRpc]
	private void HurtRpc( float damage )
	{
		EventRunner.RunLocal( GrubsEvent.GrubHurtEvent, this, damage );
	}

	[ClientRpc]
	private void DeadRpc()
	{
		EventRunner.RunLocal( GrubsEvent.GrubDiedEvent, this );
	}
}
