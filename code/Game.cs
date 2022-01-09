using Sandbox;
using Grubs.States;
using Grubs.UI;
using Grubs.Pawn;
using Grubs.Terrain;
using Grubs.Terrain.Triangulation;

namespace Grubs
{
	public partial class Game : Sandbox.Game
	{
		public static Game Instance => Current as Game;
		[Net] public StateHandler StateHandler { get; private set; }

		[Net] public IDestructableTerrain Terrain { get; private set; }

		public Game()
		{
			TriangulationHelper.BuildTriangulationTable();
			if ( IsServer )
			{
				StateHandler = new();
				_ = new HudEntity();
			}
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new Pawn.Player();
			player.Respawn();
			cl.Pawn = player;

			StateHandler.OnPlayerJoin( player );
		}

		/// <summary>
		/// Temporarily allow worms to noclip.
		/// </summary>
		public override void DoPlayerNoclip( Client player )
		{
			if ( !player.HasPermission( "noclip" ) )
				return;

			if ( player.Pawn is Pawn.Player basePlayer )
			{
				if ( basePlayer.ActiveWorm?.DevController is WormNoclipController )
				{
					Log.Info( "Noclip Mode Off" );
					basePlayer.ActiveWorm.DevController = null;
				}
				else
				{
					Log.Info( "Noclip Mode On" );
					basePlayer.ActiveWorm.DevController = new WormNoclipController();
				}
			}
		}

		public void newTerrain()
		{
			//Fix this implementation to your liking.

			TerrainEntity newTerrain = new TerrainEntity( new Vector3( 0, -4f, 1024 ), 4096f );
			newTerrain.Transmit = TransmitType.Always;
			Terrain = newTerrain;

			Terrain.ModifyRectangle( new Vector2( 0, -1024 ), new Vector2( 2048, 512 ), false );
			Terrain.ModifyCircle( new Vector2( 512, -512 ), 512, false );
		}
	}
}
