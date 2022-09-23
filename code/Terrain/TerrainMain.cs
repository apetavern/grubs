using System.IO;
using Grubs.Utils;

namespace Grubs.Terrain;

/// <summary>
/// The main class for terrains.
/// </summary>
[Category( "Setup" )]
public sealed partial class TerrainMain : Entity
{
	/// <summary>
	/// The singleton instance of <see cref="TerrainMain"/>.
	/// </summary>
	public static TerrainMain Instance = null!;

	/// <summary>
	/// The current terrain in the world.
	/// </summary>
	[Net]
	public TerrainMap Current { get; set; } = null!;

	private static List<TerrainModel> TerrainModels { get; set; } = new();

	public TerrainMain()
	{
		Instance = this;
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		base.Spawn();

		Instance = this;

		if ( GameConfig.TerrainFile != string.Empty )
		{
			var terrainFile = GameConfig.TerrainFile;
			BinaryReader? reader = null;
			try
			{
				if ( FileSystem.Mounted.FileExists( terrainFile ) )
				{
					reader = new BinaryReader( FileSystem.Mounted.OpenRead( terrainFile ) );
					Current = new TerrainMap( PremadeTerrain.Deserialize( reader ) );
				}
				else if ( FileSystem.Data.FileExists( terrainFile ) )
				{
					reader = new BinaryReader( FileSystem.Data.OpenRead( terrainFile ) );
					Current = new TerrainMap( PremadeTerrain.Deserialize( reader ) );
				}
				else
				{
					Log.Error( $"Map \"{terrainFile}\" does not exist. Reverting to random gen" );
					Current = new TerrainMap( Rand.Int( 99999 ) );
				}
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
			finally
			{
				reader?.Close();
			}
		}
		else
			Current = new TerrainMap( Rand.Int( 99999 ) );

	}
