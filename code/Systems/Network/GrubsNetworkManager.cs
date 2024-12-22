using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn;

namespace Grubs.Systems.Network;

[Title( "Grubs - Network Manager" ), Category("Grubs/Network")]
public sealed class GrubsNetworkManager : Component, Component.INetworkListener
{
	[Property] public required GameObject ClientPrefab { get; set; }
	
	protected override void OnStart()
	{
		if ( Networking.IsActive ) 
			return;
		
		var lobbyConfig = new LobbyConfig
		{
			DestroyWhenHostLeaves = false,
			AutoSwitchToBestHost = true,
			MaxPlayers = 8,
			Privacy = LobbyPrivacy.Public,
		};
		Log.Info( "Creating new Grubs lobby." );
		Networking.CreateLobby( lobbyConfig );
	}

	public void OnActive( Connection connection )
	{
		Log.Info( $"Spawning client prefab for connection {connection.DisplayName} ({connection.Id})." );
		var clientObj = ClientPrefab.Clone();
		clientObj.Name = $"Client ({connection.DisplayName})";
		clientObj.NetworkSpawn( connection );
		
		Log.Info( $"Assigning connection {connection.Id} to Client component." );
		var client = clientObj.GetComponent<Client>();
		client.OnNetworkActive( connection );
		
		BaseGameMode.Current.HandlePlayerJoined( client.Player );
	}
}
