using Sandbox;
using TerryForm.Pawn;
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
			StateHandler = new();
			Event.Register( StateHandler );
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var pawn = new Pawn.Player();
			cl.Pawn = pawn.ActiveWorm;

			StateHandler.State.OnPlayerJoin( pawn );
		}
	}
}
