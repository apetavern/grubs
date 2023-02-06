using Grubs.Player;

namespace Grubs.UI.World;

public class ChargeBar : WorldPanel
{
	public ChargeBar()
	{
		// This line should make it visible and not clip into the ground, some issues with it currently (s&box issue) 
		SceneObject.Flags.ViewModelLayer = true;

		StyleSheet.Load( "UI/Stylesheets/ChargeBar.scss" );
		Add.Panel( "charge" );

		const float width = 400;
		const float height = 150;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );
	}

	public override void Tick()
	{
		SetClass( "hidden", true );

		var activeGrub = TeamManager.Instance.CurrentTeam.ActiveGrub;

		Position = activeGrub.EyePosition + activeGrub.EyeRotation.Forward * 40f;
		Rotation = Rotation.LookAt( Vector3.Right );
	}
}
