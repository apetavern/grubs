using Sandbox;
using TerryForm.States.SubStates;

namespace TerryForm.States
{
	public partial class PlayingState : BaseState
	{
		public override string StateName => "PLAYING";
		public override int StateDurationSeconds => 1200;
		[Net] public Turn Turn { get; set; }

		protected override void OnStart()
		{
			PickNextPlayer();
		}

		public void OnTurnFinished()
		{
			if ( CheckWinCondition() )
			{
				StateHandler.Instance?.ChangeState( new EndState() );
				return;
			}

			PickNextPlayer();
		}

		protected void PickNextPlayer()
		{
			RotatePlayers();

			Turn = new Turn();
			Turn?.InitFrom( StateHandler.Instance?.Players[0], this );

			Turn?.Start();
		}

		public override void OnTick()
		{
			base.OnTick();

			Turn?.OnTick();
		}

		private bool CheckWinCondition()
		{
			var players = StateHandler.Instance?.Players;

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
			StateHandler.Instance?.ChangeState( new PlayingState() );
		}
	}
}
