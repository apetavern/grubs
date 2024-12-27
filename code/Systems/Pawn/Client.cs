using Grubs.Common;

namespace Grubs.Systems.Pawn;

[Title( "Client" ), Category( "Grubs/Pawn" )]
public sealed class Client : LocalComponent<Client>
{
	private static readonly Logger Log = new( "Client" );

	public Connection Owner => Network.Owner;
	
	[Sync( SyncFlags.FromHost )]
	public Player Player { get; set; }
	
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
		Log.Info( $"Network active for {connection.DisplayName}." );
		
		var playerObj = PlayerPrefab.Clone();
		playerObj.Name = $"Player ({connection.DisplayName})";
		playerObj.NetworkSpawn( connection );
		
		var player = playerObj.GetComponent<Player>();
		player.SetClient( this );

		Player = player;
	}
}
