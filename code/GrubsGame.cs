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

namespace Grubs;

public sealed partial class GrubsGame : GameManager
{
	/// <summary>
	/// This game.
	/// </summary>
	public static GrubsGame Instance => Current as GrubsGame;

	[Net]
	public World GameWorld { get; set; }

	public GrubsGame()
	{
		if ( Game.IsClient )
		{
			_ = new UI.GrubsHud();
		}
		else
		{
			Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// s&box moment
		if ( Game.IsServer && GameWorld is null )
			GameWorld = new World();

		Sound.FromScreen( To.Single( client ), "beach_ambience" );

		GamemodeSystem.Instance?.OnClientJoined( client );
		UI.TextChat.AddInfoChatEntry( $"{client.Name} has joined" );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		GamemodeSystem.Instance?.OnClientDisconnect( client, reason );
		UI.TextChat.AddInfoChatEntry( $"{client.Name} has left ({reason})" );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		GamemodeSystem.Instance.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
	}

	public override void OnVoicePlayed( IClient client )
	{
		UI.PlayerList.Current?.OnVoicePlayed( client );
	}

	[GrubsEvent.Game.End]
	public void OnGameOver()
	{
		GamemodeSystem.Instance.Delete();
		GamemodeSystem.SetupGamemode();
		GameWorld.Reset();
	}

	[GameEvent.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		GamemodeSystem.SetupGamemode();
	}
}
