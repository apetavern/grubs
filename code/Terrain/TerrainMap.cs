using System.Diagnostics;

namespace Grubs.Terrain;

public partial class TerrainMap
{
	public bool[,] TerrainGrid { get; set; }

	public int Width { get; set; } = 100;

	public int Height { get; set; } = 100;

	public bool HasBorder { get; set; } = true;

	private readonly float surfaceLevel = 0.55f;
	private readonly int borderWidth = 5;

	public TerrainMap()
	{
		GenerateTerrainGrid();
	}

	public TerrainMap( int width, int height ) : this()
	{
		Width = width;
		Height = height;
	}

	private void GenerateTerrainGrid()
	{
		TerrainGrid = new bool[Width, Height];
		for ( int x = 0; x < Width; x++ )
			for ( int z = 0; z < Height; z++ )
				TerrainGrid[x, z] = Noise.Simplex( x, z ) > surfaceLevel;

		if ( !HasBorder )
			return;

		bool[,] borderedMap = new bool[Width + borderWidth * 2, Height + borderWidth * 2];
		for ( int x = 0; x < borderedMap.GetLength( 0 ); x++ )
			for ( int z = 0; z < borderedMap.GetLength( 1 ); z++ )
			{
				if ( x >= borderWidth && x < Width + borderWidth && z >= borderWidth && z < Height + borderWidth )
				{
					borderedMap[x, z] = TerrainGrid[x - borderWidth, z - borderWidth];
				}
				else
				{
					borderedMap[x, z] = true;
				}
			}

		TerrainGrid = borderedMap;
	}

	public void DestructSphere( Vector2 midpoint, int size )
	{
		Log.Info( $"{Host.Name} {midpoint.x} {midpoint.y}" );
		float x = midpoint.x / 15f;
		float y = midpoint.y / 15f;

		for ( int i = 0; i < TerrainGrid.GetLength( 0 ); i++ )
		{
			for ( int j = 0; j < TerrainGrid.GetLength( 1 ); j++ )
			{
				var xDiff = Math.Abs( x - i );
				var yDiff = Math.Abs( y - j );
				var d = Math.Sqrt( Math.Pow( xDiff, 2 ) + Math.Pow( yDiff, 2 ) );
				if ( d < size )
				{
					// Log.Info( Host.Name + " // deleting " + i + "," + j );
					TerrainGrid[i, j] = false;
				}
			}
		}
	}
}
