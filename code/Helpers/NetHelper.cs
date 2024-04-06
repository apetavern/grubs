using Grubs.Terrain;

namespace Grubs.Helpers;

[Title( "Grubs - Network Helper" ), Category( "Networking" )]
public class NetHelper : Component, Component.INetworkListener
{
	// public static NetHelper Instance { get; set; }
	[Property] public required GameObject PlayerPrefab { get; set; }

	// public NetHelper()
	// {
	// 	Instance = this;
	// }

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
		GrubsTerrain.Instance.SendMeMissing( conn.Id, 1, 0 );
	}

	private Transform FindSpawnLocation()
	{
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		var pos = Random.Shared.FromArray( spawnPoints )?.Transform.World ?? Transform.World;
		return pos.WithScale( 1f );
	}
}
