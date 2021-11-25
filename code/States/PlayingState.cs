using Sandbox;
using TerryForm.States.SubStates;

namespace TerryForm.States
{
	public partial class PlayingState : BaseState
	{
		public override string StateName => "PLAYING";
		public override int StateDurationSeconds => 1200;
		public Turn Turn { get; set; }
		public static Pawn.Player ActivePlayer { get; set; }

		protected override void OnStart()
		{
			PickNextPlayer();
		}

		public void OnTurnFinished()
		{
			Log.Info( $"{ActivePlayer.Name} turn has finished." );

			if ( CheckWinCondition() )
			{
				Game.StateHandler.ChangeState( new EndState() );
				return;
			}

			PickNextPlayer();
		}

		protected void PickNextPlayer()
		{
			RotatePlayers();
			ActivePlayer = Game.StateHandler?.Players[0];

			Turn = new Turn( ActivePlayer, this );
			Turn?.Start();
		}

		public override void OnTick()
		{
			base.OnTick();

			Turn?.OnTick();
		}

		private bool CheckWinCondition()
		{
			var players = Game.StateHandler.Players;

			var anyPlayerAlive = false;
			foreach ( var player in players )
			{
				if ( player.IsAlive ) anyPlayerAlive = true;
			}

			if ( !anyPlayerAlive )
			{
				return true;
			}

			return false;
		}

		// Debug method for changing current state to PlayingState.
		[ServerCmd]
		public static void PlayState()
		{
			Game.StateHandler.ChangeState( new PlayingState() );
		}
	}
}
