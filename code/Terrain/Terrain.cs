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

			Vector2 center = new Vector2( 0, -Quadtree.Extents * 0.5f );
			Vector2 extents = new Vector2( Quadtree.Extents - 64f, Quadtree.Extents * 0.5f - 64f );
			SDF rect = new Rectangle( center, extents, SDF.MergeType.Add );
			Update( rect );

			for ( int i = 0; i < 50; i++ )
			{
				Vector2 circleOffset = Vector2.Random * extents;
				circleOffset.y *= 0.15f;
				SDF circle = new Circle( circleOffset, Rand.Float( 64, 256f ), SDF.MergeType.Subtract );
				Update( circle );
			}
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
