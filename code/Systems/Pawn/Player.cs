using Grubs.Common;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Systems.Pawn;

[Title( "Player" ), Category( "Grubs/Pawn" )]
public sealed class Player : LocalComponent<Player>
{
	public static IEnumerable<Player> All => Game.ActiveScene.GetAllComponents<Player>();
	
	[Sync( SyncFlags.FromHost )]
	public Client Client { get; private set; }
	
	[Property]
	public GameObject GrubPrefab { get; private set; }

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local = this;
	}

	public void SetClient( Client client )
	{
		Client = client;
	}

	public void AddGrub( Vector3 spawnPosition )
	{
		Log.Info( $"Adding new grub for player {Client} at {spawnPosition}." );
		
		var grubObj = GrubPrefab.Clone();
		grubObj.WorldPosition = spawnPosition;
		grubObj.Network.SetOrphanedMode( NetworkOrphaned.Host );
		grubObj.NetworkSpawn( Client.Connection );
		
		var grub = grubObj.GetComponent<Grub>();
		grub.SetOwner( this );
		Log.Info( $"Created {grub}." );
	}

	public override string ToString()
	{
		return $"Player ({Client.Connection.DisplayName})";
	}
}
