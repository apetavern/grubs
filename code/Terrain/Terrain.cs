using System.Collections.Generic;
using Sandbox;
using System;
using System.Linq;

namespace Grubs.Terrain
{
	public static partial class Terrain
	{
		public static List<Deformation> Deformations = new();
		public static Dictionary<Tuple<int, int>, TerrainChunk> Chunks { get; private set; } = new();
		public const int ChunksWide = 12;
		public const int ChunksHigh = 6;
		public const int ChunkResolution = 8; // how many cubes (squared) per chunk
		public const int ChunkWidth = 128; // width per chunk

		// dont touch these
		public const int NoiseWidth = ChunksWide * ChunkResolution + 1;
		public const int NoiseHeight = ChunksHigh * ChunkResolution + 1;

		public const float Seed = 151244125f;

		public static float[,,] NoiseMap { get; private set; } = GenerateNoiseMap();

		// [Event.Hotload]
		public static void Hotload()
		{
			NoiseMap = GenerateNoiseMap();
		}

		public static float[,,] GenerateNoiseMap()
		{
			NoiseMap = new float[NoiseWidth, 3, NoiseHeight];

			Log.Info( $"Generating new noise map on {(Host.IsServer ? "server" : "client")}" );

			float sizeStep = ChunkWidth / ChunkResolution;
			Vector3 mins = -new Vector3( (ChunksWide - 1) * 0.5f * ChunkWidth, 0, (ChunksHigh - 1) * 0.5f * ChunkWidth );
			for ( int x = 0; x < NoiseWidth; x++ )
			{
				for ( int z = 0; z < NoiseHeight; z++ )
				{
					Vector3 worldPos = mins + new Vector3( x * sizeStep, 0, z * sizeStep );
					Vector3 noisePos = worldPos * 0.005f;
					float noise = Noise.Perlin( noisePos.x, Seed, noisePos.z );
					noise = (noise * 0.75f + Noise.Perlin( noisePos.x * 0.5f, Seed + 112314f, noisePos.z * 0.5f ) * 0.25f);

					foreach ( Deformation deform in Deformations )
					{
						float level = SDF.Sphere( deform.Position, deform.Radius, worldPos.WithY( deform.Position.y ) );
						if ( deform.Boolean )
						{
							level = level * 0.05f + deform.Radius * 0.025f;
							noise = MathF.Min( noise, level );
						}
					}

					NoiseMap[x, 0, z] = -0.1f;
					NoiseMap[x, 1, z] = noise;
					NoiseMap[x, 2, z] = noise;
					//NoiseMap[x, 3, z] = 0.1f;
				}
			}

			return NoiseMap;
		}

		public static void UpdateNoiseMap( Deformation deform )
		{
			float sizeStep = ChunkWidth / ChunkResolution;
			Vector3 mins = -new Vector3( (ChunksWide - 1) * 0.5f * ChunkWidth, 0, (ChunksHigh - 1) * 0.5f * ChunkWidth );
			for ( int x = 0; x < NoiseWidth; x++ )
			{
				for ( int z = 0; z < NoiseHeight; z++ )
				{
					Vector3 worldPos = mins + new Vector3( x * sizeStep, 0, z * sizeStep );
					float noise = NoiseMap[x, 1, z];

					float level = SDF.Sphere( deform.Position, deform.Radius, worldPos.WithY( deform.Position.y ) );
					if ( deform.Boolean )
					{
						level = level * 0.05f + deform.Radius * 0.025f;
						noise = MathF.Min( noise, level );

						if ( noise < 0f )
						{
							NoiseMap[x, 1, z] = noise;
							NoiseMap[x, 2, z] = noise;
						}
					}
				}
			}
		}

		public static void DrawNoiseMap()
		{
			float sizeStep = ChunkWidth / ChunkResolution;

			Vector3 mins = -new Vector3( (ChunksWide - 1) * 0.5f * ChunkWidth, 0, (ChunksHigh - 1) * 0.5f * ChunkWidth );
			for ( int x = 0; x < NoiseWidth; x++ )
			{
				for ( int z = 0; z < NoiseHeight; z++ )
				{
					Vector3 worldPos = mins + new Vector3( x * sizeStep, 0, z * sizeStep );
					if ( NoiseMap[x, 1, z] > MarchingCubes.GroundLevel )
						DebugOverlay.Circle( worldPos, Rotation.FromYaw( 90f ), sizeStep * 0.5f, Color.White );
				}
			}
		}

		[Event.Frame]
		public static void Frame()
		{
			// DrawNoiseMap();
		}

		public static void Deform( Vector3 origin, bool boolean, float radius )
		{
			Deformation deform = new Deformation( origin, boolean, radius );
			Deformations.Add( deform );
			UpdateNoiseMap( deform );

			foreach ( TerrainChunk chunk in Entity.All.Where( e => e is TerrainChunk ) )
			{
				//Chunks.TryGetValue( key, out var chunk );

				float width = 1 << 6;
				width *= MathF.Sqrt( 3f );

				float distance = (chunk.Position - origin).WithY( 0 ).Length;

				if ( distance < width + radius )
				{
					//DebugOverlay.Sphere( chunk.Position, width, Color.White, true, 10f );
					//chunk.Delete();
					//Chunks[key] = TerrainChunk.Create( key.Item1, key.Item2 );
					chunk.Generate();
				}

			}

			if ( Host.IsServer )
				ClientDeform( origin, boolean, radius );
		}

		[ClientRpc]
		public static void ClientDeform( Vector3 origin, bool boolean, float radius )
		{
			Deform( origin, boolean, radius );
		}

		public static void ClearDeforms()
		{
			Deformations.Clear();

			GenerateNoiseMap();

			if ( Host.IsServer )
				ClearDeformsClient();
		}

		[ClientRpc]
		public static void ClearDeformsClient()
		{
			ClearDeforms();
		}

		[ServerCmd( "tf_terrain_clear" )]
		public static void ClearDeformsCmd()
		{
			ClearDeforms();
		}

		[ServerCmd( "tf_terrain_gen" )]
		public static void Generate()
		{
			for ( int x = 0; x < ChunksWide; x++ )
			{
				for ( int y = 0; y < ChunksHigh; y++ )
				{
					var key = new Tuple<int, int>( x, y );
					if ( Chunks.TryGetValue( key, out TerrainChunk existingChunk ) )
					{
						existingChunk.Generate();
					}
					else
					{
						Chunks.Add( key, TerrainChunk.Create( x, y ) );
					}
				}
			}
		}
	}
}
