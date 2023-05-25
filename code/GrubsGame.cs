global using Sandbox;
global using Sandbox.Diagnostics;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Utility;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;

namespace Grubs;

public sealed partial class GrubsGame : GameManager
{
	/// <summary>
	/// This game.
	/// </summary>
	public static GrubsGame Instance => Current as GrubsGame;

	[Net] public Terrain Terrain { get; set; }

	/// <summary>
	/// The colors that are already in use by other players. 
	/// </summary>
	/// <returns></returns>
	[Net] public IDictionary<long, Player.ColorId> TakenColors { get; private set; }

	public GrubsGame()
	{
		if ( Game.IsClient )
		{
			_ = new UI.GrubsHud();
		}
		else
		{
			PrecacheFiles();
			Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		Sound.FromScreen( To.Single( client ), "beach_ambience" );

		GamemodeSystem.Instance?.OnClientJoined( client );
		UI.TextChat.AddInfoChatEntry( $"{client.Name} has joined" );

		FetchInteractionsClient( To.Single( client ) );

		TakenColors[client.SteamId] = Player.ColorId.Undecided;
	}

	[ConCmd.Server]
	public static void SetPlayTimeServer( int clientNetworkId, float hours )
	{
		var ent = FindByIndex( clientNetworkId );
		if ( ent is not IClient client )
			return;
		if ( client.Pawn is not Player player )
			return;

		player.PlayTime = hours;
	}

	[ClientRpc]
	public async Task FetchInteractionsClient()
	{
		var pkg = await FetchPackageInfo();
		SetPlayTimeServer( Game.LocalClient.NetworkIdent, pkg.Interaction.Seconds / 3600f );
	}

	public static async Task<Package> FetchPackageInfo()
	{
		var pkg = await Package.Fetch( "apetavern.grubs", false );
		if ( pkg is null )
			return null;

		return pkg;
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		GamemodeSystem.Instance?.OnClientDisconnect( client, reason );
		UI.TextChat.AddInfoChatEntry( $"{client.Name} has left ({reason})" );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		GamemodeSystem.Instance.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
	}

	public override void PostLevelLoaded()
	{
		Terrain = new Terrain();
	}

	public override void OnVoicePlayed( IClient client )
	{
		UI.PlayerList.Current?.OnVoicePlayed( client );
	}

	private void PrecacheFiles()
	{
		foreach ( var clothing in ResourceLibrary.GetAll<Clothing>() )
		{
			// These are the only types of clothing that can be applied to Grubs.
			if ( clothing.Category != Clothing.ClothingCategory.Hair &&
				clothing.Category != Clothing.ClothingCategory.Hat &&
				clothing.Category != Clothing.ClothingCategory.Facial &&
				clothing.Category != Clothing.ClothingCategory.Skin )
				continue;

			// Cache all their stuff.
			if ( !string.IsNullOrEmpty( clothing.Model ) )
				Precache.Add( clothing.Model );
			if ( !string.IsNullOrEmpty( clothing.SkinMaterial ) )
				Precache.Add( clothing.SkinMaterial );
			if ( !string.IsNullOrEmpty( clothing.EyesMaterial ) )
				Precache.Add( clothing.EyesMaterial );
		}
	}

	[GrubsEvent.Game.End]
	public void OnGameOver()
	{
		Game.ResetMap( new Entity[] { Terrain, Terrain.SdfWorld } );
		GamemodeSystem.Instance.Delete();
		GamemodeSystem.SetupGamemode();

		Terrain.Reset();
	}

	[GameEvent.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		GamemodeSystem.SetupGamemode();
	}

	[ConCmd.Server]
	public static void SetTerrainEnvironmentType( string option )
	{
		if ( Enum.TryParse<GrubsConfig.TerrainEnvironmentType>( option, true, out var result ) )
		{
			GrubsConfig.WorldTerrainEnvironmentType = result;
			Instance.Terrain.Reset();
		}
		else
		{
			Log.Error( "We had trouble setting this option. Contact a dev!" );
		}
	}

	[ConCmd.Server]
	public static void SetTerrainType( string option )
	{
		if ( Enum.TryParse<GrubsConfig.TerrainType>( option, true, out var result ) )
		{
			GrubsConfig.WorldTerrainType = result;
			Instance.Terrain.Reset();
		}
		else
		{
			Log.Error( "We had trouble setting this option. Contact a dev!" );
		}
	}

	[ConCmd.Server]
	public static void SetTerrainTexture( string option )
	{
		if ( Enum.TryParse<GrubsConfig.TerrainTexture>( option, true, out var result ) )
		{
			GrubsConfig.WorldTerrainTexture = result;
			Instance.Terrain.Reset();
		}
		else
		{
			Log.Error( "We had trouble setting this option. Contact a dev!" );
		}
	}
}
