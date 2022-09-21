global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;

global using System;
global using System.Collections.Generic;
global using System.Linq;
using System.IO;
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
public sealed partial class GrubsGame : Game
{
	/// <summary>
	/// This game.
	/// </summary>
	public new static GrubsGame Current => (Game.Current as GrubsGame)!;

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
			BaseState.Init();
			_ = new EventRunner();

			// Set the Rand seed and Seed property so the terrain generation seed is synced between server and client.
			Rand.SetSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
			Seed = Rand.Int( 99999 );
		}

		if ( IsClient )
		{
			_ = new Hud();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		BaseState.Instance.ClientJoined( client );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		BaseState.Instance.ClientDisconnected( cl, reason );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		BaseState.Instance.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		BaseState.Instance.FrameSimulate( cl );
	}

	/// <summary>
	/// Regenerates the <see cref="TerrainMap"/> grid.
	/// </summary>
	public static void RegenerateGrid()
	{
		TerrainMain.Current.GenerateTerrainGrid();
	}

	/// <summary>
	/// Regenerates the <see cref="TerrainModel"/>.
	/// </summary>
	public static void RegenerateMap()
	{
		TerrainMain.RefreshDirtyChunks();
	}

	[Event.Tick]
	private static void Tick()
	{
		BaseState.Instance.Tick();
	}

	[ConCmd.Admin( "save_gmap" )]
	public static void SaveMap( string fileName, bool preserveSettings = true )
	{
		var writer = new BinaryWriter( FileSystem.Data.OpenWrite( fileName ) );
		try
		{
			PremadeTerrain.Serialize( writer, TerrainMain.Current, preserveSettings );
		}
		finally
		{
			writer.Close();
		}
	}

	/// <summary>
	/// Admin command to skip a <see cref="Team"/>s turn.
	/// </summary>
	[ConCmd.Admin]
	public static void SkipTurn()
	{
		if ( BaseState.Instance is not BaseGamemode gamemode )
			return;

		gamemode.NextTurnTask = gamemode.NextTurn();
	}

	/// <summary>
	/// Admin command to kill the currently active <see cref="Grub"/>.
	/// </summary>
	[ConCmd.Admin]
	public static void KillActiveGrub()
	{
		if ( BaseState.Instance is not BaseGamemode gamemode )
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
		if ( BaseState.Instance is not BaseGamemode gamemode )
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
		if ( BaseState.Instance is not BaseGamemode gamemode )
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
		if ( BaseState.Instance is not BaseGamemode gamemode )
			return;

		_ = new WeaponCrate
		{
			Position = gamemode.TeamManager.CurrentTeam.ActiveGrub.Position + Vector3.Up * 100
		};
	}

	/// <summary>
	/// The client receiver to explode a portion of the terrain map.
	/// </summary>
	/// <param name="midpoint">The center point of the explosion.</param>
	/// <param name="size">How big the explosion was.</param>
	[ClientRpc]
	public static void ExplodeClient( Vector2 midpoint, float size )
	{
		if ( TerrainMain.Current.DestructSphere( midpoint, size ) )
			RegenerateMap();
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
		if ( TerrainMain.Current.DestructLine( startpoint, endPoint, width ) )
			RegenerateMap();
	}
}
