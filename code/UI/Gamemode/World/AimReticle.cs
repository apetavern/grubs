using Grubs.Player;

namespace Grubs.UI.World;

public sealed class AimReticle : WorldPanel
{
	public AimReticle()
	{
		// This line should make it visible and not clip into the ground, some issues with it currently (s&box issue) 
		SceneObject.Flags.ViewModelLayer = true;

		StyleSheet.Load( "UI/Stylesheets/AimReticle.scss" );
		Add.Image( "materials/reticle/reticle.png" );
		Rotation = Rotation.RotateAroundAxis( Vector3.Up, 90f );
	}

	public override void Tick()
	{
		base.Tick();

		var activeGrub = TeamManager.Instance.CurrentTeam.ActiveGrub;
		if ( activeGrub.ActiveChild is null || !activeGrub.ActiveChild.HasReticle )
		{
			SetClass( "hidden", true );
			return;
		}

		/*if ( !activeGrub.Controller.IsGrounded || !activeGrub.Velocity.IsNearlyZero( 2.5f ) )
		{
			SetClass( "hidden", true );
			return;
		}*/

		SetClass( "hidden", false );
		Position = activeGrub.EyePosition + activeGrub.EyeRotation.Forward * 80f;
		Rotation = Rotation.RotateAroundAxis( Vector3.Forward, 0.25f );
	}
}
