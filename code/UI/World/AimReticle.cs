namespace Grubs;

public class AimReticle : WorldPanel
{
	public AimReticle()
	{
		SceneObject.Flags.ViewModelLayer = true;

		StyleSheet.Load( "UI/World/AimReticle.scss" );
		Add.Image( "materials/reticle/reticle.png" );
		Rotation = Rotation.RotateAroundAxis( Vector3.Up, 90f );
	}

	public override void Tick()
	{
		if ( Game.LocalPawn is not Player player )
		{
			SetClass( "hidden", true );
			return;
		}

		var activeGrub = player.ActiveGrub;
		if ( activeGrub == null )
		{
			SetClass( "hidden", true );
			return;
		}

		if ( activeGrub.ActiveWeapon is null || !activeGrub.ActiveWeapon.ShowReticle )
		{
			SetClass( "hidden", true );
			return;
		}

		if ( !activeGrub.Controller.IsGrounded || !activeGrub.Velocity.IsNearlyZero( 2.5f ) )
		{
			SetClass( "hidden", true );
			return;
		}

		SetClass( "hidden", false );

		Position = activeGrub.EyePosition + activeGrub.EyeRotation.Forward * 80f * activeGrub.Facing;
		Rotation = Rotation.RotateAroundAxis( Vector3.Forward, 0.25f );
	}
}
