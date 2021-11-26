using Sandbox;
using System.Linq;
using TerryForm.Weapons;

namespace TerryForm.Pawn
{
	public partial class Worm : Sandbox.Player
	{
		[Net] public Weapon EquippedWeapon { get; set; }
		[Net] public bool IsCurrentTurn { get; set; }
		[Net] public bool IsAlive { get; set; }

		// Temporary to allow respawning, we don't want respawning later so we can remove this.
		private TimeSince TimeSinceDied { get; set; }

		public override void Respawn()
		{
			SetModel( "models/citizenworm.vmdl" );

			IsAlive = true;

			Controller = new WormController();
			Animator = new WormAnimator();

			base.Respawn();
		}

		protected void EquipWeapon( Weapon weapon )
		{
			EquippedWeapon?.Delete();

			EquippedWeapon = weapon;
			EquippedWeapon?.OnCarryStart( this );
		}

		public void DressFromClient( Client cl )
		{
			var clothes = new Clothing.Container();
			clothes.LoadFromClient( cl );

			// Skin tone
			var skinTone = clothes.Clothing.FirstOrDefault( model => model.Model == "models/citizen/citizen.vmdl" );
			SetMaterialGroup( skinTone?.MaterialGroup );

			// We only want the hair so we won't use the logic built into Clothing
			var hair = clothes.Clothing.FirstOrDefault( item => item.Category == Clothing.ClothingCategory.Hair );

			if ( hair is null )
				return;

			var ent = new AnimEntity( hair.Model, this );

			if ( !string.IsNullOrEmpty( hair.MaterialGroup ) )
				ent.SetMaterialGroup( hair.MaterialGroup );
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

			SimulateActiveChild( cl, EquippedWeapon );
		}

		public override void SimulateActiveChild( Client cl, Entity child )
		{
			base.SimulateActiveChild( cl, child );

			(child as Weapon)?.SetWeaponEnabled( Velocity.WithZ( 0 ).IsNearZeroLength && GroundEntity is not null );
		}

		public void OnTurnStarted()
		{
			IsCurrentTurn = true;

			// Temporarily select a weapon from all weapons.
			var randWeapons = Library.GetAll<Weapon>()
				.Where( weapon => !weapon.IsAbstract )
				.ToList();

			EquipWeapon( Library.Create<Weapon>( Rand.FromList( randWeapons ) ) );
			EquippedWeapon?.SetWeaponEnabled( true );
		}

		public void OnTurnEnded()
		{
			IsCurrentTurn = false;

			if ( Health < 0 )
				OnKilled();

			EquippedWeapon?.SetWeaponEnabled( false );
		}

		public override void CreateHull()
		{
			CollisionGroup = CollisionGroup.Player;
			AddCollisionLayer( CollisionLayer.Player );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 32 ) );
			MoveType = MoveType.MOVETYPE_WALK;
			EnableHitboxes = true;
		}

		public override void OnKilled()
		{
			base.OnKilled();

			IsAlive = false;

			EnableDrawing = false;
			EnableAllCollisions = false;

			EquippedWeapon?.OnOwnerKilled();
		}
	}
}
