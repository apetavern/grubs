using Grubs.Pawn;
using Grubs.Terrain;

namespace Grubs.Gamemodes.Modes;

[Title( "Grubs - FFA" ), Category( "Grubs" )]
public sealed class FreeForAllGamemode : Gamemode
{
	public override string GamemodeName => "Free For All";

	[HostSync] public Guid ActivePlayerId { get; set; }

	internal override async void Initialize()
	{
		State = GameState.Menu;

		if ( Connection.Local == Connection.Host )
		{
			GrubsTerrain.Instance.Network.TakeOwnership();
			GrubsTerrain.Instance.Init();
		}
	}

	internal override void Start()
	{
		var players = Scene.GetAllComponents<Player>();
		foreach ( var player in players )
		{
			for ( var i = 0; i < GrubsConfig.GrubCount; i++ )
			{
				var go = player.GrubPrefab.Clone();
				go.NetworkSpawn( player.Network.OwnerConnection );

				var grub = go.Components.Get<Grub>();
				grub.Player = player;

				var spawn = GrubsTerrain.Instance.FindSpawnLocation();
				grub.Transform.Position = spawn;

				if ( i == 0 )
				{
					SetActiveGrub( player.Id, grub.Id );
				}
			}

			var inv = player.Components.Get<PlayerInventory>();
			inv.Player = player;
			inv.InitializeWeapons();
		}

		ActivePlayerId = players.ElementAt( 0 ).Id;
		Started = true;
		State = GameState.Playing;
	}

	[Broadcast]
	public void SetActiveGrub( Guid playerId, Guid grubId )
	{
		var pc = Scene.Directory.FindComponentByGuid( playerId );
		var gc = Scene.Directory.FindComponentByGuid( grubId );

		if ( pc is not Player player || gc is not Grub grub )
			return;

		player.ActiveGrub = grub;
	}
}
