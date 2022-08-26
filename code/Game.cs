global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

global using System;
global using System.Collections.Generic;
global using System.Linq;

using Grubs.Player;
using Grubs.States;
using Grubs.Terrain;
using Grubs.UI;
using Grubs.Utils.Event;

namespace Grubs;

public partial class GrubsGame : Game
{
	public new static GrubsGame Current => Game.Current as GrubsGame;

	[Net]
	public BaseState CurrentState { get; set; }

	public TerrainMap TerrainMap { get; set; }

	public TerrainModel TerrainModel { get; set; }

	[Net]
	public int Seed { get; set; }

	public GrubsGame()
	{
		// Uncomment below to use WIP Terrain!
		InitializeTerrainMap();

		if ( IsServer )
		{
			CurrentState = new WaitingState();
			_ = new EventRunner();
		}

		if ( IsClient )
		{
			_ = new Hud();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		CurrentState?.ClientJoined( client );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CurrentState?.ClientDisconnected( cl, reason );
	}

	[Event.Tick]
	private void Tick()
	{
		CurrentState?.Tick();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		CurrentState?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		CurrentState?.FrameSimulate( cl );
	}

	public void InitializeTerrainMap()
	{
		TerrainMap = new TerrainMap( 100, 100 );

		TerrainModel = new TerrainModel();
	}

	[ConCmd.Server( name: "ScrambleMap" )]
	public static void ScrambleMap()
	{
		if ( Host.IsClient )
			return;

		Current.Seed = Rand.Int( 99999 );
		// Log.Info( Current.Seed );
		Current.TerrainMap.Seed = Current.Seed;
		SetSeedClient( To.Everyone, Current.Seed );
		Current.RegenerateGrid();
		Current.RegenerateMap();
	}

	[ClientRpc]
	public static void SetSeedClient( int seed )
	{
		Current.TerrainMap.Seed = seed;
		Current.RegenerateGrid();
		Current.RegenerateMap();
	}

	public void RegenerateGrid()
	{
		TerrainMap.GenerateTerrainGrid();
	}

	public void RegenerateMap()
	{
		TerrainModel.GenerateMeshAndWalls();
	}

	/// <summary>
	/// Test command for flipping a bit in the terrain grid.
	/// </summary>
	/// <param name="x">The x position of the bit to flip.</param>
	/// <param name="z">The z position of the bit to flip.</param>
	[ConCmd.Server( name: "FlipGridBit" )]
	public static void FlipGridBit( int x, int z )
	{
		var grid = Current.TerrainMap.TerrainGrid;
		Log.Info( $"{Host.Name} PRE: " + grid[x, z] );
		grid[x, z] = !grid[x, z];
		Log.Info( $"{Host.Name} POST: " + grid[x, z] );

		Current.RegenerateMap();
		FlipGridBitClient( To.Everyone, x, z );
	}

	[ClientRpc]
	public static void FlipGridBitClient( int x, int z )
	{
		var grid = Current.TerrainMap.TerrainGrid;
		Log.Info( $"{Host.Name} PRE: " + grid[x, z] );
		grid[x, z] = !grid[x, z];
		Log.Info( $"{Host.Name} POST: " + grid[x, z] );

		Current.RegenerateMap();
	}

	[ClientRpc]
	public static void ExplodeClient( Vector2 midpoint, int size )
	{
		Current.TerrainMap.DestructSphere( midpoint, size );
		Current.RegenerateMap();
	}
}
