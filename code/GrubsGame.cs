global using Editor;
global using Sandbox;
global using Sandbox.Diagnostics;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Utility;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;
using Sandbox.Csg;
using static Sandbox.Event;

namespace Grubs;

public sealed partial class GrubsGame : GameManager
{
	/// <summary>
	/// This game.
	/// </summary>
	public static GrubsGame Instance => Current as GrubsGame;

	[Net]
	public World World { get; set; }


	public GrubsGame()
	{
		if ( Game.IsClient )
		{
			// _ = new Hud();
		}
		else
		{
			Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		if ( World == null )
		{
			SpawnWorld();
		}

		GamemodeSystem.Instance?.OnClientJoined( client );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );

		GamemodeSystem.Instance?.OnClientDisconnect( client, reason );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
	}

	private void SpawnWorld()
	{
		Assert.True( Game.IsServer );

		World = new World();
	}

	[Event.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		Log.Info( " PostEntitySpawn " );
		GamemodeSystem.SetupGamemode();
	}
}
