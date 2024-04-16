namespace Grubs.Terrain;

[Title( "World - Water" ), Category( "Grubs" )]
public sealed class Water : Component
{
	[Property] public int Resolution { get; set; } = 100;
	[Property] public int EdgeLength { get; set; } = 50;
	[Property] public Material? Material { get; set; }

	protected override void OnStart()
	{
		var mesh = new Mesh();

		var vertices = new List<SimpleVertex>();
		var indices = new List<int>();
		var index = 0;

		for ( var y = 0; y < Resolution; y++ )
		{
			for ( var x = 0; x < Resolution; x++ )
			{
				var i = x + y * Resolution;
				var p = new Vector2( x, y ) / (Resolution - 1);
				var point = Vector3.Up + (p.x - 0.5f) * EdgeLength * Vector3.Forward +
				            (p.y - 0.5f) * EdgeLength * Vector3.Left;
				vertices.Add( new SimpleVertex( point, Vector3.Up, Vector3.Forward, new Vector2( 0f ) ) );

				if ( x != Resolution - 1 && y != Resolution - 1 )
				{
					indices.Add( i );
					indices.Add( i + Resolution + 1 );
					indices.Add( i + Resolution );
					indices.Add( i );
					indices.Add( i + 1 );
					indices.Add( i + Resolution + 1 );
					index += 6;
				}
			}
		}

		mesh.CreateVertexBuffer( vertices.Count, SimpleVertex.Layout, vertices );
		mesh.CreateIndexBuffer( indices.Count, indices );

		var builder = new ModelBuilder();
		var model = builder.AddMesh( mesh )
			.AddCollisionMesh( vertices.Select( v => v.position ).ToArray(), indices.ToArray() )
			.Create();

		var mr = Components.GetOrCreate<ModelRenderer>();
		mr.Model = model;
		mr.SetMaterial( Material );
		mr.Enabled = true;
	}

	protected override void OnPreRender()
	{
		var sceneObject = GameObject.Components.Get<ModelRenderer>().SceneObject;
		sceneObject.Flags.IsTranslucent = true;
		sceneObject.Flags.IsOpaque = false;
	}
}
