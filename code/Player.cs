using Sandbox;
using TerryForm.Weapons;

namespace TerryForm
{
	public partial class Player : Sandbox.Player
	{
		[Net] public Weapon EquippedWeapon { get; set; }

		public override void Respawn()
		{
			SetModel( "models/citizenworm.vmdl" );

			Controller = new WormController();
			Camera = new Camera();

			EquipWeapon( new Weapon() );

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
			//Simulate our currently equipped weapon.
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
