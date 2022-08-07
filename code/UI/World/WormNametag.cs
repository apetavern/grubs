using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametag : WorldPanel
{
	public Worm Worm { get; set; }

	private static Vector3 Offset => Vector3.Up * 48;

	private Label name;
	private Label health;

	public WormNametag()
	{
		StyleSheet.Load( "/UI/Stylesheets/WormNametag.scss" );

		name = Add.Label( "Name", "worm-name" );
		health = Add.Label( "0", "worm-health" );

		float width = 600;
		float height = 300;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;
	}

	public override void Tick()
	{
		base.Tick();

		Move();
		UpdateLabels();
	}

	private void Move()
	{
		if ( !Worm.IsValid || Worm is null )
		{
			Delete();
			return;
		}

		Position = Worm.Position + Offset;
		Rotation = Rotation.LookAt( Vector3.Right );

		var player = Local.Pawn as GrubsPlayer;
		if ( player.Camera is GrubsCamera camera )
		{
			WorldScale = 1.5f + (camera.Distance / camera.MaxDistance * 2);
		}
	}

	private void UpdateLabels()
	{
		name.Text = Worm.Name.ToString();
		health.Text = Worm.Health.ToString();
	}
}
