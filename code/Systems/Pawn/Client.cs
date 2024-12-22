using Grubs.Common;

namespace Grubs.Systems.Pawn;

[Title( "Client" ), Category( "Grubs/Pawn" )]
public sealed class Client : LocalComponent<Client>
{
	[Sync( SyncFlags.FromHost )] 
	public Guid ConnectionId { get; set; }
	
	public Connection Connection => Connection.Find( ConnectionId );
	
	public Player Player { get; private set; }
	
	[Property]
	private GameObject PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local = this;
	}
	
	public void OnNetworkActive( Connection connection )
	{
		ConnectionId = connection.Id;
		
		var playerObj = PlayerPrefab.Clone();
		playerObj.Name = $"Player ({connection.DisplayName})";
		playerObj.NetworkSpawn();
		
		var player = playerObj.GetComponent<Player>();
		player.SetClient( this );

		Player = player;
	}
	
	public override string ToString()
	{
		return $"Client ({Connection.DisplayName}) on {GameObject.Name}";
	}
}
