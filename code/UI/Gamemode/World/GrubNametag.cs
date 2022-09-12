using System.Globalization;
using Grubs.Player;

namespace Grubs.UI.World;

public sealed class GrubNametag : WorldPanel
{
	public Grub Grub { get; }

	private static Vector3 Offset => Vector3.Up * 48;

	public string GrubName => Grub.Name;
	public string GrubHealth => Math.Ceiling( Grub.Health ).ToString( CultureInfo.CurrentCulture );

	private Label _healthLabel;

	public GrubNametag( Grub grub )
	{
		Grub = grub;

		StyleSheet.Load( "/UI/Stylesheets/GrubNametag.scss" );

		var name = Add.Label( "Name", "grub-name" );
		name.Bind( "text", this, nameof( GrubName ) );
		_healthLabel = Add.Label( "0", "grub-health" );
		_healthLabel.Bind( "text", this, nameof( GrubHealth ) );

		const float width = 600;
		const float height = 300;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Grub.IsValid )
		{
			Delete();
			return;
		}

		_healthLabel.SetClass( "hidden", Grub.LifeState == LifeState.Dead );

		Position = Grub.Position + Offset;
		Rotation = Rotation.LookAt( Vector3.Right );

		if ( Local.Pawn is ISpectator { Camera: GrubsCamera camera } )
			WorldScale = 1.5f + (camera.Distance / camera.MaxDistance * 2);
	}
}
