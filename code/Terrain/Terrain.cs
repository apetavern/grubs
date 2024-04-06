using Sandbox.Sdf;

namespace Grubs.Terrain;

[Title( "Grubs - Terrain" )]
public class GrubsTerrain : Component
{
	public static GrubsTerrain Instance { get; set; }
	[Property] public required Sdf2DWorld SdfWorld { get; set; }
	public Sdf2DLayer Sand { get; set; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );

	public GrubsTerrain()
	{
		Instance = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
		{
			Log.Info( SdfWorld );
		}
	}

	public async void Init()
	{
		await SdfWorld.ClearAsync();
		SdfWorld.Transform.Rotation = Rotation.FromRoll( 90f );

		var circleSdf = new CircleSdf( new Vector2( 0f, 0f ), 128f );
		await SdfWorld.AddAsync( circleSdf, Sand );
	}

	public void SubtractCircle( Vector2 center, float radius )
	{
		var circleSdf = new CircleSdf( center, radius );
		var translatedCircleSdf = circleSdf.Translate( new Vector2( 0f, -Transform.Position.z ) );
		SdfWorld?.SubtractAsync( translatedCircleSdf, Sand );
	}
}
