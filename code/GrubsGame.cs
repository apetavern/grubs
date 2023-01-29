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
public sealed class GrubsGame : GameManager
{
	/// <summary>
	/// This game.
	/// </summary>
	public new static GrubsGame Current => (GameManager.Current as GrubsGame)!;

	public GrubsGame()
	{
		Precache.Run();

		if ( Game.IsServer )
		{
			BaseState.Init();
			_ = new EventRunner();

			Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
		}

		if ( Game.IsClient )
		{
			_ = new Hud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// BaseState.Instance.ClientJoined( client );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		// BaseState.Instance.ClientDisconnected( cl, reason );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// BaseState.Instance.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		// BaseState.Instance?.FrameSimulate( cl );
	}

	[Event.Tick]
	private static void Tick()
	{
		BaseState.Instance.Tick();
	}

	[ConCmd.Admin( "save_gmap" )]
	public static void SaveMap( string fileName, bool preserveSettings = true )
	{
		if ( BaseState.Instance is not BaseGamemode gamemode )
			return;

		var writer = new BinaryWriter( FileSystem.Data.OpenWrite( fileName ) );
		try
		{
			PremadeTerrain.Serialize( writer, gamemode.TerrainMap, preserveSettings );
		}
		catch ( Exception e )
		{
			Log.Error( e );
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
			Attacker = (Entity)ConsoleSystem.Caller.Pawn,
			Damage = 9999,
			Tags = { "admin" }
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
}
