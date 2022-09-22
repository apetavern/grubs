using System.IO;
using Grubs.Utils;
using Grubs.Utils.Extensions;

namespace Grubs.Terrain;

/// <summary>
/// A custom/pre-made terrain that can be loaded into the terrain system.
/// </summary>
public sealed class PremadeTerrain
{
	/// <summary>
	/// The current version of pre-made terrains. Used for serializing/deserializing.
	/// </summary>
	public const int Version = 2;

	/// <summary>
	/// The version of the terrain that was loaded.
	/// </summary>
	public readonly int MapVersion;
	/// <summary>
	/// The width of the terrain.
	/// </summary>
	public readonly int Width;
	/// <summary>
	/// The height of the terrain.
	/// </summary>
	public readonly int Height;
	/// <summary>
	/// The grid array of the terrain.
	/// </summary>
	public readonly bool[] TerrainGrid;
	/// <summary>
	/// The settings that came with the terrain.
	/// </summary>
	public readonly Dictionary<TerrainSetting, object> Settings;

	/// <summary>
	/// Gets the <see cref="TerrainSetting.Border"/> setting. Reverts to default if not present.
	/// </summary>
	public bool HasBorder
	{
		get
		{
			if ( Settings.TryGetValue( TerrainSetting.Border, out var hasBorder ) )
				return (bool)hasBorder;

			return (bool)TerrainSetting.Border.GetDefaultValue();
		}
	}

	/// <summary>
	/// Gets the <see cref="TerrainSetting.TerrainType"/> setting. Reverts to default if not present.
	/// </summary>
	public TerrainType TerrainType
	{
		get
		{
			if ( Settings.TryGetValue( TerrainSetting.TerrainType, out var terrainType ) )
				return (TerrainType)terrainType;

			return (TerrainType)TerrainSetting.TerrainType.GetDefaultValue();
		}
	}

	/// <summary>
	/// Gets the <see cref="TerrainSetting.Scale"/> setting. Reverts to default if not present.
	/// </summary>
	public int Scale
	{
		get
		{
			if ( Settings.TryGetValue( TerrainSetting.Scale, out var scale ) )
				return (int)scale;

			return (int)TerrainSetting.Scale.GetDefaultValue();
		}
	}

	private PremadeTerrain( int version, int width, int height, bool[] terrainGrid,
		Dictionary<TerrainSetting, object> settings )
	{
		MapVersion = version;
		Width = width;
		Height = height;
		TerrainGrid = terrainGrid;
		Settings = settings;
	}

	/// <summary>
	/// Reads a pre-made terrains data and re-constructs it.
	/// </summary>
	/// <param name="reader">The <see cref="BinaryReader"/> to read the terrain data from.</param>
	/// <returns>The created terrain.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when reading a setting that is not recognized.</exception>
	public static PremadeTerrain Deserialize( BinaryReader reader )
	{
		// Get terrain version.
		var version = reader.ReadInt32();
		// Using an older version, revert to conversion methods.
		if ( version != Version )
			return ConvertTerrain( version, reader );

		// Read settings.
		var numSettings = reader.ReadInt32();
		var settings = new Dictionary<TerrainSetting, object>( numSettings );
		for ( var i = 0; i < numSettings; i++ )
		{
			switch ( (TerrainSetting)reader.ReadByte() )
			{
				case TerrainSetting.Border:
					settings.Add( TerrainSetting.Border, reader.ReadBoolean() );
					break;
				case TerrainSetting.Scale:
					settings.Add( TerrainSetting.Scale, reader.ReadInt32() );
					break;
				case TerrainSetting.TerrainType:
					var terrainTypeStr = reader.ReadString();
					if ( !Enum.TryParse( terrainTypeStr, out TerrainType terrainType ) )
					{
						Log.Error( $"Got invalid terrain type \"{terrainTypeStr}\", reverting to {GameConfig.TerrainType}" );
						terrainType = GameConfig.TerrainType;
					}

					settings.Add( TerrainSetting.TerrainType, terrainType );
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( TerrainSetting ) );
			}
		}

		// Read basic terrain data.
		var width = reader.ReadInt32();
		var height = reader.ReadInt32();
		var terrainGrid = new bool[width * height];
		var currentValue = reader.ReadBoolean();
		terrainGrid[0] = currentValue;

		// Read changes in terrain.
		var numChanges = reader.ReadInt32();
		var currentCoord = 0;
		for ( var i = 0; i < numChanges; i++ )
		{
			var coord = reader.ReadInt32();
			while ( currentCoord < coord )
			{
				terrainGrid[currentCoord] = currentValue;
				currentCoord++;
			}

			currentValue = !currentValue;
		}

		return new PremadeTerrain( 2, width, height, terrainGrid, settings );
	}

	/// <summary>
	/// Writes a terrains data.
	/// </summary>
	/// <param name="writer">The <see cref="BinaryWriter"/> to write the terrain data to.</param>
	/// <param name="map">The map whose data to write.</param>
	/// <param name="preserveSettings">Whether or not to save the <see cref="TerrainSetting"/>s alongside the terrain.</param>
	public static void Serialize( BinaryWriter writer, TerrainMap map, bool preserveSettings )
	{
		// Write version
		writer.Write( Version );
		// Write settings.
		if ( preserveSettings )
		{
			// Write last pre-made terrains settings.
			if ( map.Premade )
			{
				writer.Write( map.PremadeMap!.Settings.Count );
				foreach ( var pair in map.PremadeMap.Settings )
					writer.Write( pair.Key, pair.Value );
			}
			// Write random gen settings.
			else
			{
				var values = Enum.GetValues<TerrainSetting>();
				writer.Write( values.Length );
				foreach ( var setting in values )
					writer.Write( setting );
			}
		}
		// Don't write any settings.
		else
			writer.Write( 0 );

		// Fetch all terrain changes.
		var currentValue = map.TerrainGrid[0];
		var valueChanges = new List<int>();
		for ( var i = 0; i < map.TerrainGrid.Length; i++ )
		{
			if ( currentValue == map.TerrainGrid[i] )
				continue;

			valueChanges.Add( i );
			currentValue = !currentValue;
		}

		// Write terrain data.
		writer.Write( map.Width );
		writer.Write( map.Height );
		writer.Write( map.TerrainGrid[0] );
		writer.Write( valueChanges.Count );
		foreach ( var index in valueChanges )
			writer.Write( index );
	}

	private static PremadeTerrain ConvertTerrain( int mapVersion, BinaryReader reader )
	{
		return mapVersion switch
		{
			1 => ConvertV1( reader ),
			_ => throw new ArgumentException( nameof( mapVersion ) )
		};
	}

	private static PremadeTerrain ConvertV1( BinaryReader reader )
	{
		// Read settings.
		var numSettings = reader.ReadInt32();
		var settings = new Dictionary<TerrainSetting, object>( numSettings );
		for ( var i = 0; i < numSettings; i++ )
		{
			switch ( (TerrainSetting)reader.ReadByte() )
			{
				case TerrainSetting.Border:
					settings.Add( TerrainSetting.Border, reader.ReadBoolean() );
					break;
				case TerrainSetting.Scale:
					settings.Add( TerrainSetting.Scale, reader.ReadInt32() );
					break;
				case TerrainSetting.TerrainType:
					var terrainTypeStr = reader.ReadString();
					if ( !Enum.TryParse( terrainTypeStr, out TerrainType terrainType ) )
					{
						Log.Error( $"Got invalid terrain type \"{terrainTypeStr}\", reverting to {GameConfig.TerrainType}" );
						terrainType = GameConfig.TerrainType;
					}

					settings.Add( TerrainSetting.TerrainType, terrainType );
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( TerrainSetting ) );
			}
		}

		// Read basic terrain data.
		var width = reader.ReadInt32();
		var height = reader.ReadInt32();
		var terrainGrid = new bool[width, height];
		var currentValue = reader.ReadBoolean();
		terrainGrid[0, 0] = currentValue;

		// Read changes in terrain.
		var numChanges = reader.ReadInt32();
		var (currentX, currentY) = (0, 0);
		for ( var i = 0; i < numChanges; i++ )
		{
			var coordX = reader.ReadInt32();
			var coordY = reader.ReadInt32();

			while ( currentY < coordY )
			{
				for ( var j = currentX; j < width; j++ )
					terrainGrid[j, currentY] = currentValue;

				currentX = 0;
				currentY++;
			}

			for ( var j = currentX; j < coordX; j++ )
				terrainGrid[j, currentY] = currentValue;

			currentX = coordX;
			currentY = coordY;
			currentValue = !currentValue;
		}

		// Convert 2D terrain grid to 1D index array.
		var correctTerrainGrid = new bool[width * height];
		for ( var y = 0; y < height; y++ )
			for ( var x = 0; x < width; x++ )
				correctTerrainGrid[Dimensions.Convert2dTo1d( x, y, width )] = terrainGrid[x, y];

		return new PremadeTerrain( Version, width, height, correctTerrainGrid, settings );
	}
}
