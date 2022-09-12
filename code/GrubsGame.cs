global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

global using System;
global using System.Collections.Generic;
global using System.Linq;
using Grubs.Crates;
using Grubs.Player;
using Grubs.States;
using Grubs.Terrain;
using Grubs.UI;
using Grubs.Utils.Event;
using Grubs.Weapons.Base;
using Precache = Grubs.Utils.Precache;

namespace Grubs;

/// <summary>
/// The official Sbox GOTY 2022, Grubs!
/// </summary>
public partial class GrubsGame : Game
{
	/// <summary>
	/// This game.
	/// </summary>
	public new static GrubsGame Current => (Game.Current as GrubsGame)!;

	/// <summary>
	/// The current <see cref="BaseState"/> that the game is in.
	/// </summary>
	[Net]
	public BaseState CurrentState { get; set; } = null!;

	/// <summary>
	/// The current <see cref="BaseGamemode"/> the game is in.
	/// </summary>
	public BaseGamemode CurrentGamemode
	{
		get
		{
			if ( CurrentState is not BaseGamemode gamemode )
			{
				Log.Error( "Attempted to get gamemode when there is none running" );
				return default!;
			}

			return gamemode;
		}
	}

	/// <summary>
	/// The <see cref="TerrainMap"/> in the world.
	/// </summary>
	public TerrainMap TerrainMap { get; set; } = null!;

	/// <summary>
	/// The model of the terrain map.
	/// </summary>
	public TerrainModel TerrainModel { get; set; } = null!;

	/// <summary>
	/// The seed used to create the terrain map.
	/// </summary>
	[Net]
	public int Seed { get; set; }

	public GrubsGame()
	{
		Precache.Run();

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

		CurrentState.ClientJoined( client );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CurrentState.ClientDisconnected( cl, reason );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		CurrentState.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		CurrentState.FrameSimulate( cl );
	}

	/// <summary>
	/// Regenerates the <see cref="TerrainMap"/> grid.
	/// </summary>
	public void RegenerateGrid()
	{
		TerrainMap.GenerateTerrainGrid();
	}

	/// <summary>
	/// Regenerates the <see cref="TerrainModel"/>.
	/// </summary>
	public void RegenerateMap()
	{
		TerrainModel.GenerateMeshAndWalls();
	}

	[Event.Tick]
	private void Tick()
	{
		CurrentState.Tick();
	}

	/// <summary>
	/// Admin command to skip a <see cref="Team"/>s turn.
	/// </summary>
	[ConCmd.Admin]
	public static void SkipTurn()
	{
		if ( Current.CurrentState is not BaseGamemode gamemode )
			return;

		gamemode.NextTurnTask = gamemode.NextTurn();
	}

	/// <summary>
	/// Admin command to kill the currently active <see cref="Grub"/>.
	/// </summary>
	[ConCmd.Admin]
	public static void KillActiveGrub()
	{
		if ( Current.CurrentState is not BaseGamemode gamemode )
			return;

		var activeGrub = gamemode.TeamManager.CurrentTeam.ActiveGrub;
		gamemode.UseTurn( false );
		activeGrub.TakeDamage( new DamageInfo
		{
			Attacker = ConsoleSystem.Caller.Pawn,
			Damage = 9999,
			Flags = DamageFlags.Crush
		} );
		activeGrub.ApplyDamage();
	}

	/// <summary>
	/// Admin command to give ammo the active <see cref="Team"/>.
	/// </summary>
	/// <param name="weaponName">The name of the weapon to give ammo to.</param>
	/// <param name="amount">The amount of ammo to give.</param>
	[ConCmd.Admin]
	public static void GiveAmmo( string weaponName, int amount )
	{
		if ( Current.CurrentState is not BaseGamemode gamemode )
			return;

		var activeTeam = gamemode.TeamManager.CurrentTeam;
		activeTeam.GiveAmmo( WeaponAsset.All.First( weaponAsset => weaponAsset.WeaponName == weaponName ), amount );
	}

	/// <summary>
	/// Admin command to spawn a <see cref="HealthCrate"/> above the active <see cref="Grub"/>s head.
	/// </summary>
	[ConCmd.Admin]
	public static void SpawnHealthCrate()
	{
		if ( Current.CurrentState is not BaseGamemode gamemode )
			return;

		_ = new HealthCrate
		{
			Position = gamemode.TeamManager.CurrentTeam.ActiveGrub.Position + Vector3.Up * 100
		};
	}

	/// <summary>
	/// Admin command to spawn a <see cref="WeaponCrate"/> above the active <see cref="Grub"/>s head.
	/// </summary>
	[ConCmd.Admin]
	public static void SpawnWeaponCrate()
	{
		if ( Current.CurrentState is not BaseGamemode gamemode )
			return;

		_ = new WeaponCrate
		{
			Position = gamemode.TeamManager.CurrentTeam.ActiveGrub.Position + Vector3.Up * 100
		};
	}

	/// <summary>
	/// Test command to change the seed on the <see cref="TerrainMap"/> and regenerate it.
	/// </summary>
	[ConCmd.Admin]
	public static void ScrambleMap()
	{
		if ( Host.IsClient )
			return;

		Rand.SetSeed( (int)Time.Now );
		Current.Seed = Rand.Int( 99999 );
		Current.TerrainMap.Seed = Current.Seed;
		SetSeedClient( To.Everyone, Current.Seed );
		Current.RegenerateGrid();
		Current.RegenerateMap();
	}

	[ClientRpc]
	public static void InitializeTerrainClient()
	{
		Current.TerrainMap = new TerrainMap();
		Current.TerrainModel = new TerrainModel();
	}

	/// <summary>
	/// The client receiver to set the seed for the terrain.
	/// </summary>
	/// <param name="seed">The new seed for the terrain.</param>
	[ClientRpc]
	public static void SetSeedClient( int seed )
	{
		Current.TerrainMap.Seed = seed;
		Current.RegenerateGrid();
		Current.RegenerateMap();
	}

	/// <summary>
	/// The client receiver to explode a portion of the terrain map.
	/// </summary>
	/// <param name="midpoint">The center point of the explosion.</param>
	/// <param name="size">How big the explosion was.</param>
	[ClientRpc]
	public static void ExplodeClient( Vector2 midpoint, float size )
	{
		if ( Current.TerrainMap.DestructSphere( midpoint, size ) )
			Current.RegenerateMap();
	}

	/// <summary>
	/// Destruct a sphere in the <see cref="TerrainMap"/>.
	/// </summary>
	/// <param name="startpoint">The start point of the line to be destructed.</param>
	/// <param name="endPoint">The endpoint of the line to be destructed.</param>
	/// <param name="width">The size (radius) of the sphere to be destructed.</param>
	[ClientRpc]
	public static void LineClient( Vector3 startpoint, Vector3 endPoint, float width )
	{
		if ( Current.TerrainMap.DestructLine( startpoint, endPoint, width ) )
			Current.RegenerateMap();
	}
}
