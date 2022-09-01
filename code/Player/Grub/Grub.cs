using System.Threading.Tasks;
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
public partial class Grub : AnimatedEntity, IResolvable
{
	/// <summary>
	/// The grubs movement controller.
	/// </summary>
	[Net, Predicted]
	public GrubController Controller { get; private set; }

	/// <summary>
	/// The grubs animator.
	/// </summary>
	[Net, Predicted]
	public GrubAnimator Animator { get; private set; }

	/// <summary>
	/// The currently active weapon the grub is using.
	/// </summary>
	[Net, Predicted]
	public GrubWeapon? ActiveChild { get; private set; }

	/// <summary>
	/// The last weapon the grub was using.
	/// </summary>
	[Net, Predicted]
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

	/// <summary>
	/// Returns whether or not this grub has been damaged.
	/// </summary>
	public bool HasBeenDamaged => _damageQueue.Count != 0;

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

	public Grub()
	{
		Transmit = TransmitType.Always;
	}

	public void Spawn( Client? cl = null )
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Name = Rand.FromArray( GameConfig.GrubNames );
		Health = 100;
		EnableHitboxes = true;

		Controller = new GrubController();
		Animator = new GrubAnimator();

		if ( cl is not null )
			DressFromClient( cl );
		SetHatVisible( true );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( !IsServer )
			return;

		if ( !_takeDamage )
		{
			_damageQueue.Enqueue( info );
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
		Gravestone = new Gravestone( this ) { Owner = this };

		ChatBox.AddInformation( To.Everyone, DeathReason.ToString(), $"avatar:{Team.ActiveClient.PlayerId}" );
		EventRunner.RunLocal( GrubsEvent.GrubDiedEvent, this );
		DeadRpc( To.Everyone );

		if ( GrubsCamera.GetTarget() == this )
			GrubsCamera.SetTarget( null );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
			Gravestone?.Delete();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Gravestone?.Simulate( cl );
		Controller?.Simulate( cl, this, Animator );

		if ( LifeState != LifeState.Dead )
		{
			foreach ( var zone in TerrainZone.All.OfType<DamageZone>() )
			{
				if ( !zone.InstantKill || !zone.InZone( this ) )
					continue;

				GrubsGame.Current.CurrentGamemode.UseTurn();
				zone.Trigger( this );
				ApplyDamage();
			}
		}

		SimulateActiveChild( cl, ActiveChild );
	}

	/// <summary>
	/// Simulates the active weapon.
	/// </summary>
	/// <param name="client">The client that is predicting.</param>
	/// <param name="child">The weapon to simulate.</param>
	public virtual void SimulateActiveChild( Client client, GrubWeapon? child )
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
	public virtual void OnActiveChildChanged( GrubWeapon? previous, GrubWeapon? next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	/// <summary>
	/// Applies any damage that this grub has received.
	/// </summary>
	/// <returns>Whether or not the grub has died.</returns>
	public virtual bool ApplyDamage()
	{
		if ( !HasBeenDamaged )
			return false;

		_takeDamage = true;
		GrubsCamera.SetTarget( this );

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
		return dead;
	}

	/// <summary>
	/// Equips a new weapon for this grub.
	/// </summary>
	/// <param name="weapon">The weapon to equip.</param>
	public void EquipWeapon( GrubWeapon? weapon )
	{
		ActiveChild = weapon;
	}

	/// <summary>
	/// Dresses the grub based on the provided clients avatar.
	/// </summary>
	/// <param name="cl">The client to get the clothes of.</param>
	public void DressFromClient( Client cl )
	{
		var clothes = new ClothingContainer();
		clothes.LoadFromClient( cl );

		// Skin tone
		var skinTone = clothes.Clothing.FirstOrDefault( model => model.Model == "models/citizenworm.vmdl" );
		SetMaterialGroup( skinTone?.MaterialGroup );

		// We only want the hair/hats so we won't use the logic built into Clothing
		var items = clothes.Clothing.Where( item =>
			item.Category == Clothing.ClothingCategory.Hair ||
			item.Category == Clothing.ClothingCategory.Hat
		);

		if ( !items.Any() )
			return;

		foreach ( var item in items )
		{
			var ent = new AnimatedEntity( item.Model, this );

			// Add a tag to the hat so we can reference it later.
			if ( item.Category == Clothing.ClothingCategory.Hat
				|| item.Category == Clothing.ClothingCategory.Hair )
				ent.Tags.Add( "head" );

			if ( !string.IsNullOrEmpty( item.MaterialGroup ) )
				ent.SetMaterialGroup( item.MaterialGroup );
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
		{
			hat.EnableDrawing = visible;
		}
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
