using Sandbox;
using TerryForm.Pawn;

namespace TerryForm.Weapons
{
	public class Railgun : Weapon
	{
		public override string WeaponName => "Railgun";
		public override string ModelPath => "models/weapons/railgun/railgun.vmdl";
		public override HoldPose HoldPose => HoldPose.Rifle;
		public override float PrimaryRate => 2f;

		public override void Fire()
		{
			var tempTrace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward.Normal * 400 ).Ignore( this ).Run();

			DebugOverlay.Line( tempTrace.StartPos, tempTrace.EndPos );

			base.Fire();
		}
	}
}
