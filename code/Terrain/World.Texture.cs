using Sandbox.Csg;
using System;
using System.Numerics;

namespace Grubs;

public partial class World
{
	/// <summary>
	/// Experimental.
	/// </summary>
	private void SetupTextureWorld()
	{
		GenerateTextureWorld( "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString().ToLower() + ".tlvl" );
	}

	private void GenerateTextureWorld( string TexturePath )
	{
		ResourceLibrary.TryGet( TexturePath, out TextureLevel map );
		if ( map != null )
		{
			PossibleSpawnPoints.Clear();

			var _WorldLength = map.texture.Width * 16;
			var _WorldHeight = map.texture.Height * 16;
			var pointsX = map.texture.Width;
			var pointsZ = map.texture.Height;

			SetupWater( _WorldLength, _WorldHeight );
			SetupKillZone( _WorldHeight );

			//CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( _WorldLength, WorldWidth, _WorldHeight ), position: new Vector3( 0, 0, -_WorldHeight / 2 ) );
			if ( map.background == null )
			{
				CsgBackground.Add( CoolBrush, RockMaterial, scale: new Vector3( _WorldLength, WorldWidth, _WorldHeight ), position: new Vector3( 0, 72, -_WorldHeight / 2 ) );
			}

			_terrainGrid = new float[pointsX, pointsZ];

			Color32[] pixels = map.texture.GetPixels().Reverse().ToArray();



			List<Vector3> points = new List<Vector3>();
			List<Vector3> lineStarts = new List<Vector3>();
			List<Vector3> lineEnds = new List<Vector3>();

			float lineWidth = 44.0f; // width of each CSG line

			for ( int i = 0; i < pixels.Length; i++ )
			{
				int x = i % pointsX;
				int y = i / pointsX;

				// check if the current pixel is part of a CSG line
				if ( pixels[i].a > 0 )
				{
					var min = new Vector3( (x * 16) - 16, -16, (y * 16) - 16 );
					var max = new Vector3( (x * 16) + 16, 16, (y * 16) + 16 );

					// Offset by position.
					min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
					max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );

					// add the current point to the list of points
					points.Add( (min + max) / 2 );

					// check if the next pixel in the same row is also part of the line
					int nextX = x;
					while ( nextX < pointsX - 1 && pixels[i + 1].a > 0 )
					{
						nextX++;
						i++;
					}
					if ( nextX > x )
					{
						// add a line between the current point and the last point in the same row
						min = new Vector3( (nextX * 16) - 16, -16, (y * 16) - 16 );
						max = new Vector3( (nextX * 16) + 16, 16, (y * 16) + 16 );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );

						points.Add( (min + max) / 2 );

						lineStarts.Add( points[points.Count - 2] );
						lineEnds.Add( points[points.Count - 1] );
					}
				}
				else
				{
					var min = new Vector3( (x * 16) - 16, -16, (y * 16) - 16 );
					var max = new Vector3( (x * 16) + 16, 16, (y * 16) + 16 );

					// Offset by position.
					min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
					max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
					PossibleSpawnPoints.Add( (min + max) / 2 );
				}
			}

			// create CSG lines from the list of line start and end points



			for ( int i = 0; i < lineStarts.Count; i++ )
			{
				Vector3 start = lineStarts[i];
				Vector3 end = lineEnds[i];
				float size = lineWidth;
				Quaternion rotation = Rotation.Identity;
				AddLine( start, end, size, rotation );
			}

			/*for ( int i = 0; i < mines.Count; i++ )
			{
				AddDefaultCube( mines[i], maxes[i] );
			}*/

			/*var min = new Vector3();
			var max = new Vector3();
			var n = 0;
			int index = 0;

			var paddedRes = 16;// + (16 * 0.5f);

			for ( var x = 0; x < pointsX; x++ )
			{
				for ( var z = 0; z < pointsZ; z++ )
				{
					index = z * pointsX + x;

					n = pixels[index].a;

					_terrainGrid[x, z] = n;

					// Add solid where alpha == 255
					if ( _terrainGrid[x, z] > 0 )
					{
						var depth = (_terrainGrid[x, z] / 255f) * 16;
						min = new Vector3( (x * 16) - paddedRes, -depth, (z * 16) - paddedRes );
						max = new Vector3( (x * 16) + paddedRes, 16, (z * 16) + paddedRes );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, depth - 8, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, depth - 8, _WorldHeight );
						AddDefault( min, max );
						//SubtractDefault( min, max );
					}
					else
					{
						min = new Vector3( (x * 16), -16, (z * 16) );
						max = new Vector3( (x * 16), 16, (z * 16) );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );

						var avg = (min + max) / 2;
						PossibleSpawnPoints.Add( avg );
					}
				}
			}*/

			if ( map.background != null )
			{
				_terrainGrid = new float[pointsX, pointsZ];

				pixels = map.background.GetPixels().Reverse().ToArray();

				points.Clear();
				lineStarts.Clear();
				lineEnds.Clear();

				lineWidth = 46.0f; // width of each CSG line

				for ( int i = 0; i < pixels.Length; i++ )
				{
					int x = i % pointsX;
					int y = i / pointsX;

					// check if the current pixel is part of a CSG line
					if ( pixels[i].a > 0 )
					{
						var min = new Vector3( (x * 16) - 16, -16, (y * 16) - 16 );
						var max = new Vector3( (x * 16) + 16, 16, (y * 16) + 16 );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, -64, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, -64, _WorldHeight );

						// add the current point to the list of points
						points.Add( (min + max) / 2 );

						// check if the next pixel in the same row is also part of the line
						int nextX = x;
						while ( nextX < pointsX - 1 && pixels[i + 1].a > 0 )
						{
							nextX++;
							i++;
						}
						if ( nextX > x )
						{
							// add a line between the current point and the last point in the same row
							min = new Vector3( (nextX * 16) - 16, -16, (y * 16) - 16 );
							max = new Vector3( (nextX * 16) + 16, 16, (y * 16) + 16 );

							// Offset by position.
							min -= new Vector3( _WorldLength / 2, -64, _WorldHeight );
							max -= new Vector3( _WorldLength / 2, -64, _WorldHeight );

							points.Add( (min + max) / 2 );

							lineStarts.Add( points[points.Count - 2] );
							lineEnds.Add( points[points.Count - 1] );
						}
					}
				}

				// create CSG lines from the list of line start and end points



				for ( int i = 0; i < lineStarts.Count; i++ )
				{
					Vector3 start = lineStarts[i];
					Vector3 end = lineEnds[i];
					float size = lineWidth;
					Quaternion rotation = Rotation.Identity;
					AddBackgroundLine( start, end, size, rotation );
				}
			}
		}
	}

}
