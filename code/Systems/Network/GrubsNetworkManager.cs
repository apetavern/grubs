using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn;

namespace Grubs.Systems.Network;

[Title( "Grubs - Network Manager" ), Category("Grubs/Network")]
public sealed class GrubsNetworkManager : Component, Component.INetworkListener
{
	[Property] public required GameObject PlayerPrefab { get; set; }
	
	protected override void OnStart()
	{
		if ( Networking.IsActive ) 
			return;
		
		var lobbyConfig = new LobbyConfig
		{
			DestroyWhenHostLeaves = false,
			AutoSwitchToBestHost = false,
			MaxPlayers = 8,
			Privacy = LobbyPrivacy.Public,
		};
		Log.Info( "Creating new Grubs lobby." );
		Networking.CreateLobby( lobbyConfig );
	}

	public void OnActive( Connection connection )
	{
		connection.CanRefreshObjects = true;
		
		Log.Info( $"Spawning client prefab for connection {connection.DisplayName} ({connection.Id})." );
		var playerObj = PlayerPrefab.Clone();
		playerObj.Name = $"Client ({connection.DisplayName})";
		playerObj.Network.SetOwnerTransfer( OwnerTransfer.Fixed );
		playerObj.Network.SetOrphanedMode( NetworkOrphaned.Host );
		playerObj.NetworkSpawn( connection );
		
		Log.Info( $"Assigning connection {connection.Id} to Client component." );
		var player = playerObj.GetComponent<Player>();
		
		BaseGameMode.Current.HandlePlayerJoined( player );
	}

	public void OnDisconnected( Connection connection )
	{
		var player = Player.All.FirstOrDefault( player => player.Network.Owner == connection );
		Log.Info( $"Found player: {player}." );
		if ( !player.IsValid() )
			return;
		
		Log.Info( $"Handling disconnect for connection {connection.DisplayName}." );
		player.IsDisconnected = true;
		BaseGameMode.Current.HandlePlayerLeft( player );
	}
}
