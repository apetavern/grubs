using Grubs.Terrain;
using Grubs.Utils;
using Grubs.Utils.Event;
using Grubs.Weapons;

namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Worm : AnimatedEntity
{
	[Net, Predicted]
	public WormController Controller { get; set; }

	[Net, Predicted]
	public WormAnimator Animator { get; set; }

	[Net, Predicted]
	public GrubsWeapon ActiveChild { get; set; }

	[Net, Predicted]
	public GrubsWeapon LastActiveChild { get; set; }

	public Team Team => Owner as Team;

	private readonly Queue<DamageInfo> _damageQueue = new();

	public bool IsTurn
	{
		get
		{
			if ( Owner is not Team team )
				return false;

			return team.ActiveWorm == this && team.IsTurn;
		}
	}

	public Worm()
	{
		Transmit = TransmitType.Always;
	}

	public void Spawn( Client cl )
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );
		Name = Rand.FromArray( GameConfig.WormNames );
		Health = 100;

		Controller = new WormController();
		Animator = new WormAnimator();

		DressFromClient( cl );
		SetHatVisible( true );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( !IsServer )
			return;

		if ( !Velocity.IsNearlyZero( 0.1f ) )
		{
			_damageQueue.Enqueue( info );
			return;
		}

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;

		if ( Health <= 0 || LifeState != LifeState.Alive )
			return;

		Health -= info.Damage;
		EventRunner.RunLocal( GrubsEvent.WormHurtEvent, this, info.Damage );
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
		// TODO: Hide the worm instead of deleting it. Possible revive mechanic?
		EnableDrawing = false;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );

		if ( IsTurn )
			SimulateActiveChild( cl, ActiveChild );

		if ( IsServer && _damageQueue.Count != 0 && Velocity.IsNearlyZero( 0.1f ) )
			TakeDamage( _damageQueue.Dequeue() );
	}

	public virtual void SimulateActiveChild( Client client, GrubsWeapon child )
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

	public virtual void OnActiveChildChanged( GrubsWeapon previous, GrubsWeapon next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	public void EquipWeapon( GrubsWeapon weapon )
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
		EventRunner.RunLocal( GrubsEvent.WormHurtEvent, this, damage );
	}
}
