namespace Grubs;

public class GamemodeSystem
{
	public static string SelectedGamemode => GrubsConfig.Gamemode;

	private static Gamemode _instance;

	public static Gamemode Instance
	{
		get
		{
			if ( Game.IsServer )
				return _instance;

			if ( !_instance.IsValid() )
				_instance = Entity.All.FirstOrDefault( x => x is Gamemode ) as Gamemode;

			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	protected static Gamemode FetchGamemodeEntity()
	{
		var gamemode = Entity.All.FirstOrDefault( x => x is Gamemode ) as Gamemode;

		if ( !gamemode.IsValid() && !string.IsNullOrEmpty( SelectedGamemode ) )
		{
			Log.Info( $"Grubs: Attempting to find gamemode from Type - {SelectedGamemode}" );
			var gamemodeEntity = TypeLibrary.Create<Gamemode>( SelectedGamemode );
			if ( gamemodeEntity.IsValid() )
			{
				Log.Info( $"Grubs: Found gamemode from Type - {SelectedGamemode}" );
				return gamemodeEntity;
			}
		}
		else
		{
			Log.Info( "Grubs: Creating default gamemode - FFA" );
			var gamemodeEntity = TypeLibrary.Create<Gamemode>( "FreeForAll" );
			return gamemodeEntity;
		}

		return gamemode;
	}

	public static void SetupGamemode()
	{
		Instance = FetchGamemodeEntity();

		if ( Instance.IsValid() )
			Instance.Initialize();
	}
}
