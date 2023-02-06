using Grubs.Terrain;

namespace Grubs.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="TerrainSetting"/>.
/// </summary>
public static class TerrainSettingExtension
{
	/// <summary>
	/// Gets the default value of the passed <see cref="TerrainSetting"/>.
	/// </summary>
	/// <param name="setting">The <see cref="TerrainSetting"/> to get the default value of.</param>
	/// <returns>The default value of the <see cref="setting"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="setting"/> passed is invalid.</exception>
	public static object GetDefaultValue( this TerrainSetting setting )
	{
		return setting switch
		{
			TerrainSetting.Border => GameConfig.TerrainBorder,
			TerrainSetting.Scale => GameConfig.TerrainScale,
			TerrainSetting.TerrainType => GameConfig.TerrainType,
			_ => throw new ArgumentOutOfRangeException( nameof( setting ) )
		};
	}
}
