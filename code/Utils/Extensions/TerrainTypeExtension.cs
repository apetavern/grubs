using Grubs.Terrain;

namespace Grubs.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="TerrainType"/>.
/// </summary>
public static class TerrainTypeExtension
{
	/// <summary>
	/// Gets the material for the <see cref="TerrainType"/>
	/// </summary>
	/// <param name="terrainType">The <see cref="TerrainType"/> to get the material for.</param>
	/// <returns>The material associated with the <see cref="TerrainType"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the terrain type does not have an associated material.</exception>
	public static string GetMaterial( this TerrainType terrainType )
	{
		return terrainType switch
		{
			TerrainType.None => "materials/environment/cereal.vmat",
			TerrainType.Dirt => "materials/environment/dirt_rocks.vmat",
			TerrainType.Lava => "materials/environment/lava_rocks.vmat",
			TerrainType.Sand => "materials/environment/sand_shells.vmat",
			_ => throw new ArgumentException( $"Invalid {nameof( TerrainType )}", nameof( terrainType ) )
		};
	}
}
