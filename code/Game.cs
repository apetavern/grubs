global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

global using System;
global using System.Collections.Generic;
global using System.Linq;

using Grubs.Player;
using Grubs.UI;

namespace Grubs;

public partial class GrubsGame : Game
{
	public static new GrubsGame Current => Game.Current as GrubsGame;

	public GrubsGame()
	{
		if ( IsClient )
		{
			_ = new Hud();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new GrubsPlayer( client );
		client.Pawn = player;

		player.Spawn();
	}
}
