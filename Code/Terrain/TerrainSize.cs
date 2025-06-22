namespace Grubs.Terrain;

public enum TerrainSize
{
	Tiny,
	Small,
	Medium,
	Large,
	Huge
}

public static class TerrainSizeExtensions
{
	private static readonly Vector2 Tiny = new( 1024f, 512f );
	private static readonly Vector2 Small = new( 1536, 768f );
	private static readonly Vector2 Medium = new( 2048f, 1024f );
	private static readonly Vector2 Large = new( 3072f, 1536f );
	private static readonly Vector2 Huge = new( 4096f, 2048f );
	
	public static Vector2 AsVector2( this TerrainSize size )
	{
		return size switch
		{
			TerrainSize.Tiny => Tiny,
			TerrainSize.Small => Small,
			TerrainSize.Medium => Medium,
			TerrainSize.Large => Large,
			TerrainSize.Huge => Huge,
			_ => Vector2.Random
		};
	}
}
