using Sandbox;
using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Shotgun : Weapon
	{
		public override string WeaponName => "Shotgun";
		public override string ModelPath => "models/weapons/shotgun/shotgun.vmdl";
		public override HoldPose HoldPose => HoldPose.Shotgun;
		public override float PrimaryRate => 2f;

		public override void Fire()
		{
			var tempTrace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward.Normal * 200 ).Ignore( this ).Run();

			DebugOverlay.Line( tempTrace.StartPos, tempTrace.EndPos );

			base.Fire();
		}

		public override void OnFireEffects()
		{
			Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );
		}
	}
}
