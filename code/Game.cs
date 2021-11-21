using Sandbox;
using TerryForm.UI;

namespace TerryForm
{
	public partial class Game : Sandbox.Game
	{
		public static Game Instance => Current as Game;

		public Game()
		{
			_ = new HudEntity();
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var pawn = new Player();
			cl.Pawn = pawn;

			pawn.Respawn();
		}
	}
}
