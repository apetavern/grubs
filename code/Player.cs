using Sandbox;
using TerryForm.Weapons;

namespace TerryForm
{
	public partial class Player : Sandbox.Player
	{
		[Net] public Weapon EquippedWeapon { get; set; }

		public override void Respawn()
		{
			SetModel( "models/maya_testcube_100.vmdl" );

			// Need a custom controller!!
			Controller = new WalkController();
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
			DebugOverlay.Sphere( EyePos, 32f, Color.Yellow, false ); // Visualise the pawn since we don't have a model for it yet

			SimulateActiveChild( cl, EquippedWeapon ); //Simulate our currently equipped weapon.

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
