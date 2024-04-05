using Sandbox.Sdf;

namespace Grubs.Terrain;

[Title( "Grubs - Terrain" )]
public class GrubsTerrain : Component
{
	public static GrubsTerrain Instance { get; set; }
	public Sdf2DWorld? SdfWorld { get; set; }
	public Sdf2DLayer Sand { get; set; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );

	public GrubsTerrain()
	{
		Instance = this;
	}

	public void Init()
	{
		SdfWorld = Components.Create<Sdf2DWorld>();
		SdfWorld.Transform.Rotation = Rotation.FromRoll( 90f );

		var circleSdf = new CircleSdf( new Vector2( 0f, 0f ), 128f );
		SdfWorld.AddAsync( circleSdf, Sand );
	}

	public void SubtractCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		var translatedCircleSdf = circleSdf.Translate( new Vector2( 0f, -Transform.Position.z ) );
		SdfWorld?.SubtractAsync( translatedCircleSdf, Sand );
	}
}
