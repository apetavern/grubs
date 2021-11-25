using Sandbox;
using TerryForm.States;
using TerryForm.UI;

namespace TerryForm
{
	public partial class Game : Sandbox.Game
	{
		public static Game Instance => Current as Game;
		[Net] public StateHandler StateHandler { get; private set; }

		[Net] public BaseState State { get; set; }

		public Game()
		{
			if ( IsClient )
				return;

			_ = new HudEntity();
			StateHandler = new();
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new Pawn.Player( cl );
			StateHandler.OnPlayerJoin( player );
		}

		/// <summary>
		/// Temporary ServerCmd to discern what State is currently active.
		/// </summary>
		[ServerCmd]
		public static void CheckState()
		{
			Log.Trace( StateHandler.Instance?.State.StateName );
		}
	}
}
