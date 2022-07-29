global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

global using System;
global using System.Collections.Generic;
global using System.Linq;

using Grubs.Player;

namespace Grubs;

public partial class GrubsGame : Game
{
	public static new GrubsGame Current => Game.Current as GrubsGame;

	public GrubsGame()
	{

	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new GrubsPlayer();
		client.Pawn = player;

		player.Spawn();
	}
}
