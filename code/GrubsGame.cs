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
using static Sandbox.CitizenAnimationHelper;

namespace Grubs;

public sealed partial class GrubsGame : GameManager
{
	/// <summary>
	/// This game.
	/// </summary>
	public static GrubsGame Instance => Current as GrubsGame;

	[Net]
	public World World { get; set; }

	/*	[Net]
		public CsgSolid CsgWorld { get; set; }

		public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
		public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csg/default.csgmat" );*/


	public GrubsGame()
	{
		if ( Game.IsClient )
		{
			// _ = new Hud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		if ( World == null )
		{
			SpawnWorld();
		}

		client.Pawn = new Player();
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );
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
}
