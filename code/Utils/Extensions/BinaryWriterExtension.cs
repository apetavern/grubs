using System.IO;
using Grubs.Terrain;

namespace Grubs.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="BinaryWriter"/>.
/// </summary>
public static class BinaryWriterExtension
{
	/// <summary>
	/// Writes a <see cref="TerrainSetting"/> and its value.
	/// </summary>
	/// <param name="writer">The <see cref="BinaryWriter"/> to write the setting to.</param>
	/// <param name="setting">The <see cref="TerrainSetting"/> to write.</param>
	/// <param name="value">The value of the <see cref="TerrainSetting"/></param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="setting"/> passed is invalid.</exception>
	public static void Write( this BinaryWriter writer, TerrainSetting setting, object? value = null )
	{
		writer.Write( (byte)setting );
		switch ( setting )
		{
			case TerrainSetting.Border:
				writer.Write( value is null ? (bool)setting.GetDefaultValue() : (bool)value );
				break;
			case TerrainSetting.Scale:
				writer.Write( value is null ? (int)setting.GetDefaultValue() : (int)value );
				break;
			case TerrainSetting.TerrainType:
				writer.Write( (value is null ? setting.GetDefaultValue().ToString() : (string)value)! );
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( setting ), setting, null );
		}
	}
}
