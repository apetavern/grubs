using Sandbox;
using System.Linq;
using TerryForm.Weapons;

namespace TerryForm.Pawn
{
	public partial class Worm : Sandbox.Player
	{
		[Net] public Weapon EquippedWeapon { get; set; }

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
			// Simulate our currently equipped weapon.
			SimulateActiveChild( cl, EquippedWeapon );

			base.Simulate( cl );
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
