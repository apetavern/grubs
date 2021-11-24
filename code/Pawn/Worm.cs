using Sandbox;
using System.Linq;
using TerryForm.Weapons;
using TerryForm.States.SubStates;

namespace TerryForm.Pawn
{
	public partial class Worm : Sandbox.Player
	{
		[Net] public Weapon EquippedWeapon { get; set; }

		// Temporary to allow respawning, we don't want respawning later so we can remove this.
		private TimeSince TimeSinceDied { get; set; }

		public override void Respawn()
		{
			SetModel( "models/citizenworm.vmdl" );

			Controller = new WormController();
			Animator = new WormAnimator();
			Camera = new Camera();

			// Temporarily select a weapon from all weapons.
			var randWeapons = Library.GetAll<Weapon>()
				.Where( weapon => !weapon.IsAbstract )
				.ToList();

			EquipWeapon( Library.Create<Weapon>( Rand.FromList( randWeapons ) ) );

			base.Respawn();
		}

		protected void EquipWeapon( Weapon weapon )
		{
			EquippedWeapon?.Delete();

			EquippedWeapon = weapon;
			EquippedWeapon?.OnCarryStart( this );
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

			// Don't allow weapon firing if it isn't this worms turn.
			if ( Turn.Instance?.ActivePlayer.ClientId != Client.PlayerId )
				return;

			SimulateActiveChild( cl, EquippedWeapon );
		}

		public void OnTurnStarted()
		{

		}

		public void OnTurnEnded()
		{
			if ( Health < 0 )
				OnKilled();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
			EnableAllCollisions = false;

			EquippedWeapon?.OnOwnerKilled();
		}
	}
}
