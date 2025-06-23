using Grubs.Systems.LevelEditing;

namespace Grubs.Terrain;

public partial class GameTerrain
{
	[ConCmd( "gr_save_terrain" )]
	public static void SaveModificationsCmd()
	{
		Local.SaveModifications();
	}

	public void SaveModifications()
	{
		Log.Info( $"Attempting to serialize terrain from {SdfWorld.GameObject.Name}..." );
		Log.Info( $"SdfWorld has {SdfWorld.ModificationCount} modifications." );
		
		var byteStream = ByteStream.Create( 512 );
		var mods = SdfWorld.Write( ref byteStream );
		Log.Info( $"Writing {mods} modifications to level definition..." );
		LevelDefinition.Modifications = byteStream.ToArray();
		
		byteStream.Dispose();
		
		_ = WriteDefinitionToFile( LevelDefinition );
	}
	
	private async Task WriteDefinitionToFile( LevelDefinition definition )
	{
		var serializedDefinition = JsonSerializer.Serialize( definition.ToDataSchema() );

		try
		{
			var fileName = $"levels/{definition.Id.ToString()}_level.json";
			FileSystem.Data.CreateDirectory( "levels" );
			FileSystem.Data.WriteAllText( fileName, serializedDefinition );
		}
		catch ( Exception e )
		{
			Log.Error( e.Message );
		}
	}
	
	public void SerializeTerrain()
	{
		Log.Info( $"Attempting to serialize terrain from {SdfWorld.GameObject.Name}..." );
		Log.Info( $"SdfWorld has {SdfWorld.ModificationCount} modifications." );

		var fileName = $"terrain_{LevelDefinition.Id.ToString()}.json";

		try
		{
			var byteStream = ByteStream.Create( 512 );
			var mods = SdfWorld.Write( ref byteStream, 0 );
			Log.Info( $"Writing {mods} modifications to stream..." );
			var stream = FileSystem.Data.OpenWrite( fileName );
			stream.Write( byteStream.ToArray() );
			stream.Close();
			
			Log.Info( $"Done writing to {fileName}" );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}
	
	public void DeserializeTerrain( Guid id )
	{
		Log.Info( $"Attempting to load terrain for {SdfWorld.GameObject.Name}..." );
		var fileName = $"terrain_{id.ToString()}.json";
		
		if ( !FileSystem.Data.FileExists( fileName ) )
		{
			Log.Warning( $"Failed to load terrain. File does not exist." );
		}

		_ = ReadExistingTerrain( fileName );
	}
	
	private async Task ReadExistingTerrain( string fileName )
	{
		await SdfWorld.ClearAsync();
		
		try
		{
			var contents = FileSystem.Data.ReadAllBytes( fileName );
			var byteStream = ByteStream.CreateReader( contents );

			SdfWorld.ClearAndReadData( ref byteStream );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}
}
