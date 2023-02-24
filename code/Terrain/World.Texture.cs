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
			CsgBackground.Add( CoolBrush, RockMaterial, scale: new Vector3( _WorldLength, WorldWidth, _WorldHeight ), position: new Vector3( 0, 72, -_WorldHeight / 2 ) );

			_terrainGrid = new float[pointsX, pointsZ];

			Color32[] pixels = map.texture.GetPixels().Reverse().ToArray();

			var min = new Vector3();
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
					if ( _terrainGrid[x, z] == 255 )
					{
						min = new Vector3( (x * 16) - paddedRes, -16, (z * 16) - paddedRes );
						max = new Vector3( (x * 16) + paddedRes, 16, (z * 16) + paddedRes );

						// Offset by position.
						min -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						max -= new Vector3( _WorldLength / 2, 0, _WorldHeight );
						AddDefault( min, max );
						//SubtractDefault( min, max );
						/*var avg = (min + max) / 2;
						PossibleSpawnPoints.Add( avg );*/
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
			}
		}
	}
}
