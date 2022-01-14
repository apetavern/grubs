using Sandbox;
using System.Linq;
using Grubs.Utils;
using Grubs.Weapons;
using Grubs.States.SubStates;

namespace Grubs.Pawn
{
	public partial class Worm : Sandbox.Player, IAwaitResolution
	{
		[Net] public Weapon EquippedWeapon { get; set; }
		[Net] public bool IsCurrentTurn { get; set; }
		public bool IsResolved { get; set; }

		// Temporary to allow respawning, we don't want respawning later so we can remove this.
		private TimeSince TimeSinceDied { get; set; }

		public override void Respawn()
		{
			SetModel( "models/citizenworm.vmdl" );

			Controller = new WormController();
			Animator = new WormAnimator();

			// Random worm name
			Name = Rand.FromArray( GameConfig.WormNames );

			base.Respawn();
		}

		public override void CreateHull()
		{
			CollisionGroup = CollisionGroup.Player;
			AddCollisionLayer( CollisionLayer.Player );
			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule() { CenterA = new Vector3( 0, 0, 0 ), CenterB = new Vector3( 0, 0, 20 ), Radius = 12f } );
			MoveType = MoveType.MOVETYPE_WALK;
			EnableHitboxes = true;
		}

		public void DressFromClient( Client cl )
		{
			var clothes = new Clothing.Container();
			clothes.LoadFromClient( cl );

			// Skin tone
			var skinTone = clothes.Clothing.FirstOrDefault( model => model.Model == "models/citizen/citizen.vmdl" );
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
				var ent = new AnimEntity( item.Model, this );

				// Add a tag to the hat so we can reference it later.
				if ( item.Category == Clothing.ClothingCategory.Hat )
					ent.Tags.Add( "hat" );

				if ( !string.IsNullOrEmpty( item.MaterialGroup ) )
					ent.SetMaterialGroup( item.MaterialGroup );
			}
		}

		public void SetHatVisible( bool visible )
		{
			var hat = Children.OfType<AnimEntity>().FirstOrDefault( child => child.Tags.Has( "hat" ) );

			if ( hat is null )
				return;

			hat.EnableDrawing = visible;
		}

		public override void Simulate( Client cl )
		{
			/*
			 * This is the base implementation, base simulates the controllers by default and offers respawning.
			 * We don't want that so remove the respawning part later.
			 */
			if ( LifeState == LifeState.Dead )
			{
				if ( TimeSinceDied > 3 && IsServer )
				{
					Respawn();
				}

				return;
			}

			var controller = GetActiveController();
			controller?.Simulate( cl, this, GetActiveAnimator() );

			if ( IsCurrentTurn )
				SimulateActiveChild( cl, EquippedWeapon );
		}

		public void EquipWeapon( Weapon weapon )
		{
			EquippedWeapon = weapon;
		}

		public void OnTurnStarted()
		{
			IsCurrentTurn = true;
		}

		public void OnTurnEnded()
		{
			// Disable the weapon
			EquippedWeapon?.ActiveEnd( this, false );
			EquippedWeapon = null;

			// It's no longer our turn.
			IsCurrentTurn = false;

			if ( Health < 0 )
				OnKilled();
		}

		public void GiveHealth( int amount )
		{
			Health += amount;
			Health = Health.Clamp( 0, 300 );
		}

		public override void TakeDamage( DamageInfo info )
		{
			// End this worms turn immediately if it takes damage.
			if ( IsCurrentTurn )
				Turn.Instance?.ForceEnd();

			//Health -= info.Damage;

			DoKnockback( info );

			// TODO: Come back to this later. Worms should only die / apply damage once the attacking worms turn has ended.
			if ( Health <= 0 )
				OnKilled();
		}

		public void DoKnockback( DamageInfo info )
		{
			var direction = (Position - info.Position).Normal;
			var distanceFromOrigin = Vector3.DistanceBetween( Position, info.Position );

			switch ( info.Flags )
			{
				case DamageFlags.Blast:
					ApplyAbsoluteImpulse( direction * (distanceFromOrigin * 40) + (Vector3.Up * 1000) );
					break;
				case DamageFlags.Bullet:
					ApplyAbsoluteImpulse( (Vector3.Up * 600) );
					break;
			}
		}

		public override void OnKilled()
		{
			// Disable collcetions and drawing.
			LifeState = LifeState.Dead;
			EnableDrawing = false;
			EnableAllCollisions = false;

			// Explode this worm
			// Explode here.

			// Create a tombstone in this worms place.
			CreateTombstone();

			// Let the player know one of their worms has died.
			(Owner as Pawn.Player)?.OnWormKilled( this );
		}

		public void CreateTombstone()
		{
			// Create a tombstone at this worms position.
			// This will need to become it's own explodable damage dealing entity later on. 
			var tombstone = new ModelEntity( "models/gravestones/basic_gravestone/gravestone_basic.vmdl" );
			tombstone.Position = Position;
		}

		public string GetTeamClass()
		{
			int index = Client.All.ToList().IndexOf( this.Client );
			var team = "abcd"[index]; // TODO: We need a proper way of getting team colors

			return $"team-{team}";
		}
	}
}
