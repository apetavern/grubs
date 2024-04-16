using Grubs.Pawn;
using Grubs.Terrain;

namespace Grubs.Gamemodes.Modes;

[Title( "Grubs - FFA" ), Category( "Grubs" )]
public sealed class FreeForAllGamemode : Gamemode
{
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
			var go = player.GrubPrefab.Clone();
			go.NetworkSpawn();

			var grub = go.Components.Get<Grub>();
			grub.Player = player;
			player.ActiveGrub = grub;

			var inv = player.Components.Get<PlayerInventory>();
			inv.Player = player;
			inv.InitializeWeapons();

			var spawn = GrubsTerrain.Instance.FindSpawnLocation();
			go.Transform.Position = spawn;
		}

		Started = true;
		State = GameState.Playing;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		Log.Info( State );
	}
}
