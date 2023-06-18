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

	[Net] public Terrain Terrain { get; set; }

	/// <summary>
	/// The available player colors with a boolean to determine if a
	/// player has the color assigned to them.
	/// </summary>
	[Net] public IDictionary<Color, bool> PlayerColors { get; private set; }

	public GrubsGame()
	{
		if ( Game.IsClient )
		{
			_ = new UI.GrubsHud();
		}
		else
		{
			PrecacheFiles();
			Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );
			PopulatePlayerColors();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		Sound.FromScreen( To.Single( client ), "beach_ambience" );

		GamemodeSystem.Instance?.OnClientJoined( client );
		UI.TextChat.AddInfoChatEntry( $"{client.Name} has joined" );

		FetchInteractionsClient( To.Single( client ) );
	}

	[ConCmd.Server]
	public static void SetPlayTimeServer( int clientNetworkId, float hours )
	{
		var ent = FindByIndex( clientNetworkId );
		if ( ent is not IClient client )
			return;
		if ( client.Pawn is not Player player )
			return;

		player.PlayTime = hours;
	}

	[ClientRpc]
	public async Task FetchInteractionsClient()
	{
		var pkg = await FetchPackageInfo();
		SetPlayTimeServer( Game.LocalClient.NetworkIdent, pkg.Interaction.Seconds / 3600f );
	}

	public static async Task<Package> FetchPackageInfo()
	{
		var pkg = await Package.Fetch( "apetavern.grubs", false );
		if ( pkg is null )
			return null;

		return pkg;
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		GamemodeSystem.Instance?.OnClientDisconnect( client, reason );
		UI.TextChat.AddInfoChatEntry( $"{client.Name} has left ({reason})" );

		if ( client.Pawn is not Player player || GamemodeSystem.Instance.CurrentState == Gamemode.State.Playing )
			return;

		DeletePlayer( player );
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

	public override void PostLevelLoaded()
	{
		Terrain = new Terrain();
	}

	public override void OnVoicePlayed( IClient client )
	{
		UI.PlayerList.Current?.OnVoicePlayed( client );
	}

#if DEBUG
	public override void DoPlayerDevCam( IClient client )
	{
		Game.AssertServer();

		var camera = client.Components.Get<DebugCamera>( true );
		if ( camera is null )
		{
			camera = new DebugCamera();
			client.Components.Add( camera );
			return;
		}

		camera.Enabled = !camera.Enabled;
	}
#endif

	private void PrecacheFiles()
	{
		foreach ( var clothing in ResourceLibrary.GetAll<Clothing>() )
		{
			// These are the only types of clothing that can be applied to Grubs.
			if ( clothing.Category != Clothing.ClothingCategory.Hair &&
				clothing.Category != Clothing.ClothingCategory.Hat &&
				clothing.Category != Clothing.ClothingCategory.Facial &&
				clothing.Category != Clothing.ClothingCategory.Skin )
				continue;

			// Cache all their stuff.
			if ( !string.IsNullOrEmpty( clothing.Model ) )
				Precache.Add( clothing.Model );
			if ( !string.IsNullOrEmpty( clothing.SkinMaterial ) )
				Precache.Add( clothing.SkinMaterial );
			if ( !string.IsNullOrEmpty( clothing.EyesMaterial ) )
				Precache.Add( clothing.EyesMaterial );
		}
	}

	private void PopulatePlayerColors()
	{
		PlayerColors.Add( Color.FromBytes( 56, 229, 77 ), false );      // Green
		PlayerColors.Add( Color.FromBytes( 192, 255, 169 ), false );    // Bright Pastel Green
		PlayerColors.Add( Color.FromBytes( 56, 118, 29 ), false );      // ForestGreen
		PlayerColors.Add( Color.FromBytes( 255, 174, 109 ), false );    // Orange
		PlayerColors.Add( Color.FromBytes( 255, 216, 89 ), false );     // Bright Yellow
		PlayerColors.Add( Color.FromBytes( 248, 249, 136 ), false );    // Yellow
		PlayerColors.Add( Color.FromBytes( 103, 234, 202 ), false );    // Cyan
		PlayerColors.Add( Color.FromBytes( 118, 103, 87 ), false );     // PastelBrown
		PlayerColors.Add( Color.FromBytes( 240, 236, 211 ), false );    // Eggshell
		PlayerColors.Add( Color.FromBytes( 232, 59, 105 ), false );     // Red
		PlayerColors.Add( Color.FromBytes( 255, 129, 172 ), false );    // Strong Pink
		PlayerColors.Add( Color.FromBytes( 251, 172, 204 ), false );    // Pink
		PlayerColors.Add( Color.FromBytes( 213, 69, 255 ), false );     // Strong Purple
		PlayerColors.Add( Color.FromBytes( 173, 162, 255 ), false );    // Purple
		PlayerColors.Add( Color.FromBytes( 33, 146, 255 ), false );     // Blue
		PlayerColors.Add( Color.FromBytes( 169, 213, 255 ), false );    // Bright Pastel Blue
	}

	/// <summary>
	/// Tries to assign an unused color to the Player.
	/// Will use <see cref="Player.DefaultColor"/> if there aren't any unused colors left.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public bool TryAssignUnusedColor( Player player )
	{
		Game.AssertServer();

		var unusedColors = GrubsGame.Instance.PlayerColors.Where( k => !k.Value ).ToArray();
		if ( unusedColors.Length > 0 )
		{
			var color = Game.Random.FromArray( unusedColors ).Key;
			GrubsGame.Instance.PlayerColors[color] = true;
			player.Color = color;
			return true;
		}

		player.Color = Player.DefaultColor;
		return false;
	}

	/// <summary>
	/// Delete the player's pawn and reset the <see cref="PlayerColors"/> color slot they were using.
	/// </summary>
	/// <param name="player"></param>
	private void DeletePlayer( Player player )
	{
		if ( PlayerColors.ContainsKey( player.Color ) )
			PlayerColors[player.Color] = false;

		player.Delete();
	}

	[ClientRpc]
	public void PlaySoundClient( string sound )
	{
		Game.AssertClient();
		PlaySound( sound );
	}

	[GrubsEvent.Game.End]
	public void OnGameOver()
	{
		Game.ResetMap( new Entity[] { Terrain, Terrain.SdfWorld } );
		GamemodeSystem.Instance.Delete();
		GamemodeSystem.SetupGamemode();

		Terrain.Reset();
	}

	[GameEvent.Entity.PostSpawn]
	public void PostEntitySpawn()
	{
		GamemodeSystem.SetupGamemode();
	}

	[ConCmd.Server]
	public static void SetConfigOption( string key, string value )
	{
		ConsoleSystem.SetValue( key, value );

		if ( key == "terrain_environment_type"
			&& GrubsConfig.WorldTerrainType == GrubsConfig.TerrainType.Generated )
			Instance.Terrain.Refresh();
		else
			Instance.Terrain.Reset();
	}
}
