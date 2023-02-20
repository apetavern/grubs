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

	public GrubsGame()
	{
		if ( Game.IsClient )
		{
			_ = new GrubsHud();
		}
		else
		{
			Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		client.Components.Create<Preferences>();

		if ( GamemodeSystem.Instance.GameWorld is null )
			GamemodeSystem.Instance.GameWorld = new World();

		Sound.FromScreen( To.Single( client ), "beach_ambience" );

		GamemodeSystem.Instance?.OnClientJoined( client );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		GamemodeSystem.Instance?.OnClientDisconnect( client, reason );
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
		PlayerInfoPanel.Current?.OnVoicePlayed( client );
	}

	[Event.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		GamemodeSystem.SetupGamemode();
	}
}
