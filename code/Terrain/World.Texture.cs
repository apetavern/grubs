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
			float resolution = 8;

			PossibleSpawnPoints.Clear();

			var _WorldLength = map.texture.Width * resolution;
			var _WorldHeight = map.texture.Height * resolution;
			var pointsX = map.texture.Width;
			var pointsZ = map.texture.Height;

			SetupWater( _WorldLength, _WorldHeight );
			SetupKillZone( _WorldHeight );

			if ( map.background == null )
			{
				CsgBackground.Add( CoolBrush, RockMaterial, scale: new Vector3( _WorldLength, WorldWidth, _WorldHeight ), position: new Vector3( 0, 72, -_WorldHeight / 2 ) );
			}

			Color32[] pixels = map.texture.GetPixels().Reverse().ToArray();

			_terrainGrid = new float[pointsX, pointsZ];

			List<Vector3> points = new List<Vector3>();
			List<Vector3> lineStarts = new List<Vector3>();
			List<Vector3> lineEnds = new List<Vector3>();

			float lineWidth = 28.0f; // width of each CSG line

			for ( int i = 0; i < pixels.Length; i++ )
			{
				int x = i % pointsX;
				int y = i / pointsX;

				// check if the current pixel is part of a CSG line
				if ( pixels[i].a > 0 )
				{
					var min = new Vector3( (x * resolution) - resolution, -16, (y * resolution) - resolution );
					var max = new Vector3( (x * resolution) + resolution, 16, (y * resolution) + resolution );

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
						min = new Vector3( (nextX * resolution) - resolution, -16, (y * resolution) - resolution );
						max = new Vector3( (nextX * resolution) + resolution, 16, (y * resolution) + resolution );

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
					var min = new Vector3( (x * resolution) - resolution, -16, (y * resolution) - resolution );
					var max = new Vector3( (x * resolution) + resolution, 16, (y * resolution) + resolution );

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

			/*List<Vector3> LineCapEnds = new List<Vector3>();
			List<Vector3> LineCapStarts = new List<Vector3>();

			for ( int i = 0; i < lineStarts.Count; i++ )
			{
				Vector3 currentEndPoint = lineEnds[i];
				Vector3 currentStartPoint = lineStarts[i];
				for ( int j = i + 1; j < lineStarts.Count; j++ )
				{
					Vector3 nextEndPoint = lineEnds[j];
					Vector3 nextStartPoint = lineStarts[j];

					float EndDist = Vector3.DistanceBetween( currentEndPoint, nextEndPoint );

					float StartDist = Vector3.DistanceBetween( currentStartPoint, nextStartPoint );

					if ( EndDist <= 18f && EndDist > 10f && !LineCapStarts.Contains( currentEndPoint ) )
					{
						LineCapStarts.Add( currentEndPoint );
						LineCapEnds.Add( nextEndPoint );
					}

					if ( StartDist <= 18f && StartDist > 10f && !LineCapStarts.Contains( currentStartPoint ) )
					{
						LineCapStarts.Add( currentStartPoint );
						LineCapEnds.Add( nextStartPoint );
					}
				}
			}

			for ( int i = 0; i < LineCapStarts.Count; i++ )
			{
				Vector3 start = LineCapStarts[i];
				Vector3 end = LineCapEnds[i];
				float size = lineWidth;
				Vector3 direction = end - start;

				if ( lineStarts.Contains( end ) )
				{
					direction = start - end;
				}

				Quaternion rotation = Rotation.LookAt( direction );
				//DebugOverlay.Line( start, end, 60f, false );
				//AddLine( start, end, size, rotation, false );
			}*/

			if ( map.background != null )
			{
				pixels = map.background.GetPixels().Reverse().ToArray();

				points.Clear();
				lineStarts.Clear();
				lineEnds.Clear();

				for ( int i = 0; i < pixels.Length; i++ )
				{
					int x = i % pointsX;
					int y = i / pointsX;

					// check if the current pixel is part of a CSG line
					if ( pixels[i].a > 0 )
					{
						var min = new Vector3( (x * resolution) - resolution, -16, (y * resolution) - resolution );
						var max = new Vector3( (x * resolution) + resolution, 16, (y * resolution) + resolution );

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
							min = new Vector3( (nextX * resolution) - resolution, -16, (y * resolution) - resolution );
							max = new Vector3( (nextX * resolution) + resolution, 16, (y * resolution) + resolution );

							// Offset by position.
							min -= new Vector3( _WorldLength / 2, -64, _WorldHeight );
							max -= new Vector3( _WorldLength / 2, -64, _WorldHeight );

							points.Add( (min + max) / 2 );

							lineStarts.Add( points[points.Count - 2] );
							lineEnds.Add( points[points.Count - 1] );
						}
					}
				}

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
