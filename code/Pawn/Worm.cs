using Sandbox;
using System.Linq;
using TerryForm.Utils;
using TerryForm.Weapons;
using TerryForm.States.SubStates;

namespace TerryForm.Pawn
{
	public partial class Worm : Sandbox.Player
	{
		[Net] public Weapon EquippedWeapon { get; set; }
		[Net] public bool IsCurrentTurn { get; set; }

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
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 32 ) );
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
			Log.Info( "Worm received health" );
			Health += amount;
		}

		public override void TakeDamage( DamageInfo info )
		{
			// End this worms turn immediately if it takes damage.
			if ( IsCurrentTurn )
				Turn.Instance?.ForceEnd();


			Log.Info( "Worm take damage" );

			Health -= info.Damage;
		}

		public override void OnKilled()
		{
			LifeState = LifeState.Dead;
			EnableDrawing = false;
			EnableAllCollisions = false;

			(Owner as Pawn.Player)?.OnWormKilled( this );
		}

		public string GetTeamClass()
		{
			int index = Client.All.ToList().IndexOf( this.Client );
			var team = "abcd"[index]; // TODO: We need a proper way of getting team colors

			return $"team-{team}";
		}
	}
}
