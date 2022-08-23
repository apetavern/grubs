using System.Globalization;
using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametag : WorldPanel
{
	public Worm Worm { get; }

	private static Vector3 Offset => Vector3.Up * 48;

	public WormNametag( Worm worm )
	{
		Worm = worm;

		StyleSheet.Load( "/UI/Stylesheets/WormNametag.scss" );

		var name = Add.Label( "Name", "worm-name" );
		name.Bind( "text", () => Worm.Name );
		var health = Add.Label( "0", "worm-health" );
		health.Bind( "text", () => Math.Ceiling( Worm.Health ).ToString( CultureInfo.CurrentCulture ) );

		const float width = 600;
		const float height = 300;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Worm is null || !Worm.IsValid )
		{
			Delete();
			return;
		}

		SetClass( "hidden", Worm.LifeState == LifeState.Dead );

		Position = Worm.Position + Offset;
		Rotation = Rotation.LookAt( Vector3.Right );

		if ( Local.Pawn is GrubsPlayer { Camera: GrubsCamera camera } )
			WorldScale = 1.5f + (camera.Distance / camera.MaxDistance * 2);
	}
}
