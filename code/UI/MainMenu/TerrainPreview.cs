namespace Grubs.UI;

public class TerrainPreview : ScenePanel
{
	public TerrainPreview() : base()
	{
		World = Game.SceneWorld;
	}

	public override void Tick()
	{
		base.Tick();

		if ( GamemodeSystem.Instance.Terrain is not Terrain terrain )
			return;

		var center = new Vector3( 0f, 0f, terrain.WorldTextureHeight / 2 );

		Camera.ZFar = 4096f;
		Camera.Position = center.WithY( -2048f );
		Camera.Rotation = Rotation.LookAt( terrain.Rotation.Left );
	}
}
