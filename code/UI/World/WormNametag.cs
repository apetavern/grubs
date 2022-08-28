using System.Globalization;
using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametag : WorldPanel
{
	public Worm Worm { get; }

	private static Vector3 Offset => Vector3.Up * 48;

	public string WormName => Worm.Name;
	public string WormHealth => Math.Ceiling( Worm.Health ).ToString( CultureInfo.CurrentCulture );

	public WormNametag( Worm worm )
	{
		Worm = worm;

		StyleSheet.Load( "/UI/Stylesheets/WormNametag.scss" );

		var name = Add.Label( "Name", "worm-name" );
		name.Bind( "text", this, nameof( WormName ) );
		var health = Add.Label( "0", "worm-health" );
		health.Bind( "text", this, nameof( WormHealth ) );

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

		if ( Local.Pawn is ISpectator { Camera: GrubsCamera camera } )
			WorldScale = 1.5f + (camera.Distance / camera.MaxDistance * 2);
	}
}
