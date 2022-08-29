using System.Threading.Tasks;
using Grubs.States;
using Grubs.Utils;
using Grubs.Utils.Event;
using Grubs.Weapons;

namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Grub : AnimatedEntity, IResolvable
{
	[Net, Predicted]
	public GrubController Controller { get; private set; }

	[Net, Predicted]
	public GrubAnimator Animator { get; private set; }

	[Net, Predicted]
	public GrubWeapon ActiveChild { get; private set; }

	[Net, Predicted]
	public GrubWeapon LastActiveChild { get; private set; }

	[Net]
	public ModelEntity Gravestone { get; private set; }

	public Team Team => Owner as Team;

	private readonly Queue<DamageInfo> _damageQueue = new();
	private bool _takeDamage;

	public bool IsTurn
	{
		get
		{
			if ( Owner is not Team team )
				return false;

			return team.ActiveGrub == this && team.IsTurn;
		}
	}

	public bool Resolved => Velocity.IsNearlyZero( 0.1f ) || LifeState == LifeState.Dead;

	public Grub()
	{
		Transmit = TransmitType.Always;
	}

	public void Spawn( Client cl = null )
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );
		Name = Rand.FromArray( GameConfig.GrubNames );
		Health = 100;

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

		LifeState = LifeState.Dying;

		// TODO: Animate death?

		ExplosionHelper.Explode( Position, this, 50 );
		LifeState = LifeState.Dead;
		// TODO: Hide the grub instead of deleting it. Possible revive mechanic?
		EnableDrawing = false;
		Gravestone = new Gravestone( this ) { Owner = this };
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

		if ( IsTurn )
			SimulateActiveChild( cl, ActiveChild );
	}

	public virtual void SimulateActiveChild( Client client, GrubWeapon child )
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

	public virtual void OnActiveChildChanged( GrubWeapon previous, GrubWeapon next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	public virtual async Task ApplyDamage()
	{
		_takeDamage = true;

		while ( _damageQueue.TryDequeue( out var damageInfo ) )
		{
			TakeDamage( damageInfo );
			await GameTask.Delay( 100 );
		}

		_takeDamage = false;
	}

	public void EquipWeapon( GrubWeapon weapon )
	{
		ActiveChild?.ShowWeapon( this, false );
		ActiveChild = weapon;
	}

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
}
