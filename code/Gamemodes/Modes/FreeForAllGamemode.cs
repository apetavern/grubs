using Grubs.Player;
using Grubs.Terrain;

namespace Grubs.Gamemodes.Modes;

[Title( "Grubs - FFA" ), Category( "Grubs" )]
public sealed class FreeForAllGamemode : Gamemode
{
	internal override async void Initialize()
	{
		if ( Connection.Local == Connection.Host )
		{
			GrubsTerrain.Instance.Network.TakeOwnership();
			GrubsTerrain.Instance.Init();
		}

		// var grubs = Scene.GetAllComponents<Grub>();
		// foreach ( var grub in grubs )
		// {
		// 	var spawn = GrubsTerrain.Instance.FindSpawnLocation();
		// 	Log.Info( spawn );
		// 	grub.Transform.Position = spawn;
		// }
	}
}
