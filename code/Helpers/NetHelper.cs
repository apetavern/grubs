namespace Grubs.Helpers;

[Title( "Grubs - Network Helper" ), Category( "Networking" )]
public class NetHelper : Component, Component.INetworkListener
{
	[Property] public required GameObject PlayerPrefab { get; set; }

	protected override async Task OnLoad()
	{
		if ( !GameNetworkSystem.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			GameNetworkSystem.CreateLobby();
		}
	}

	public void OnActive( Connection conn )
	{
		var startPosition = FindSpawnLocation();
		var player = PlayerPrefab.Clone( startPosition, name: $"Player - {conn.DisplayName}" );
		player.NetworkSpawn( conn );
	}

	private Transform FindSpawnLocation()
	{
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		var pos = Random.Shared.FromArray( spawnPoints )?.Transform.World ?? Transform.World;
		return pos.WithScale( 1f );
	}
}
