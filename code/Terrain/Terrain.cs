using System.Collections.Generic;
using Sandbox;

namespace Grubs.Terrain
{
	public struct Terrain
	{
		private static TerrainEntity entity;
		private static TerrainEntity TerrainEntity => entity ?? (entity = Entity.FindByName( "TerrainEntity" ) as TerrainEntity);
		private static IList<SDF> SDFs => TerrainEntity.SDFs;

		private static TerrainEntity InitializeEntity()
		{
			if ( Host.IsServer )
				return new TerrainEntity();

			return Entity.FindByName( "TerrainEntity" ) as TerrainEntity;
		}

		public static void Initialize() => entity = InitializeEntity();

		public static void Update( SDF sdf )
		{
			if ( Host.IsClient )
				return;

			SDFs.Add( sdf );
		}

		/// <summary>
		/// Method for generating our base terrain.
		/// </summary>
		public static void Generate()
		{
			if ( Host.IsClient )
				return;

			Reset();


			// apply a big large rectangle
			/*Vector2 rectCenter = new Vector2( 0, -Quadtree.Extents * 0.5f );
			Vector2 extents = new Vector2( Quadtree.Extents - 64f, Quadtree.Extents * 0.5f - 64f );
			SDF baseRect = new Rectangle( rectCenter, extents, SDF.MergeType.Add );
			Update( baseRect );*/

			// create a big subtraction circle
			Vector2 center = new Vector2( 0, -Quadtree.Extents );
			SDF circle = new Circle( center, Quadtree.Extents, SDF.MergeType.Add );

			// intersect the circle with perlin noise and apply it //0.008262f
			SDF perlin = new IslandPerlin( .005f, Rand.Float( 10000f ), SDF.MergeType.SmoothIntersect );
			circle.Add( perlin );

			Vector2 center2 = new Vector2( -Quadtree.Extents * 1.5f, -Quadtree.Extents * 0.5f );
			SDF circle2 = new Circle( center2, Quadtree.Extents * 0.75f, SDF.MergeType.SmoothSubtract );
			circle.Add( circle2 );

			center2 = new Vector2( Quadtree.Extents * 1.5f, -Quadtree.Extents * 0.5f );
			circle2 = new Circle( center2, Quadtree.Extents * 0.75f, SDF.MergeType.SmoothSubtract );
			circle.Add( circle2 );

			Update( circle );
		}

		[ServerCmd( "GenerateTerrain" )]
		public static void GenerateCmd()
		{
			Generate();
		}

		public static void Reset()
		{
			if ( Host.IsClient )
				return;

			TerrainEntity.Reset();
		}
	}
}
