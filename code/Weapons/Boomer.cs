using Sandbox;
using TerryForm.Pawn;
using TerryForm.Terrain;

namespace TerryForm.Weapons
{
	public class Boomer : Weapon
	{
		public override string WeaponName => "Boomer";
		public override string ModelPath => "models/weapons/shotgun/shotgun.vmdl";
		public override HoldPose HoldPose => HoldPose.Shotgun;
		public override float PrimaryRate => 1f;
		public override bool IsFiredTurnEnding => true;

		public override void Simulate( Client player )
		{
			base.Simulate( player );

			if ( IsServer )
			{
				using ( Prediction.Off() )
				{
					int input = Input.Pressed( InputButton.Attack1 ) ? 1 : (Input.Pressed( InputButton.Attack2 ) ? 0 : -1);

					var tempTrace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward.Normal * WeaponReach ).Ignore( this ).Run();
					DebugOverlay.Line( tempTrace.StartPos, tempTrace.EndPos );

					if ( input != -1 )
					{
						var position = tempTrace.EndPos;

						Color color = input == 1 ? Color.Red : Color.Green;
						DebugOverlay.Circle( position, Rotation.FromYaw( 90f ), 64f, color.WithAlpha( 0.15f ), true, 5f );
						Terrain.Terrain.Deform( position, input == 1, 128f );
					}
				}
			}
		}

		public override void OnFireEffects()
		{
			Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );
		}
	}
}
