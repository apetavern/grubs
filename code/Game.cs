using Sandbox;
using TerryForm.States;
using TerryForm.UI;

namespace TerryForm
{
	public partial class Game : Sandbox.Game
	{
		public static Game Instance => Current as Game;
		public static StateHandler StateHandler { get; private set; }

		public Game()
		{
			_ = new HudEntity();

			if ( IsClient )
				return;

			StateHandler = new();
			Event.Register( StateHandler );
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new Pawn.Player();
			player.InitializeFromClient( cl );

			StateHandler.OnPlayerJoin( player );
		}

		/// <summary>
		/// Temporary ServerCmd to discern what State is currently active.
		/// </summary>
		[ServerCmd]
		public static void CheckState()
		{
			Log.Trace( StateHandler.State.StateName );
		}
	}
}
