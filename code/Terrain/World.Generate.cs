namespace Grubs;

public partial class World
{
	private void SetupGenerateWorld()
	{
		GenerateRandomWorld();
	}

	public void GenerateRandomWorld()
	{
		CsgWorld.Add( CubeBrush, SandMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 0, -WorldHeight / 2 ) );
		CsgBackground.Add( CubeBrush, RockMaterial, scale: new Vector3( WorldLength, WorldWidth, WorldHeight ), position: new Vector3( 0, 72, -WorldHeight / 2 ) );

		PossibleSpawnPoints.Clear();
		var pointsX = (WorldLength / _resolution).CeilToInt();
		var pointsZ = (WorldHeight / _resolution).CeilToInt();

		_terrainGrid = new float[pointsX, pointsZ];

		var r = Game.Random.Int( 99999 );

		// Initialize Perlin noise grid.
		for ( var x = 0; x < pointsX; x++ )
		{
			for ( var z = 0; z < pointsZ; z++ )
			{
				var n = Noise.Perlin( (x + r) * _zoom, r, (z + r) * _zoom );
				n = Math.Abs( (n * 2) - 1 );
				_terrainGrid[x, z] = n;

				// Subtract from the solid where the noise is under a certain threshold.
				if ( _terrainGrid[x, z] < 0.1f )
				{
					// Pad the subtraction so the subtraction is more clean.
					var paddedRes = _resolution + (_resolution * 0.75f);

					var min = new Vector3( (x * _resolution) - paddedRes, -32, (z * _resolution) - paddedRes );
					var max = new Vector3( (x * _resolution) + paddedRes, 32, (z * _resolution) + paddedRes );

					// Offset by position.
					min -= new Vector3( WorldLength / 2, 0, WorldHeight );
					max -= new Vector3( WorldLength / 2, 0, WorldHeight );
					SubtractDefault( min, max );

					var avg = (min + max) / 2;
					PossibleSpawnPoints.Add( avg );
				}
			}
		}
	}
}
