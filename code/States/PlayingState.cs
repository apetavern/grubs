using Sandbox;
using Grubs.Crates;
using Grubs.States.SubStates;

namespace Grubs.States
{
	public partial class PlayingState : BaseState
	{
		public override string StateName => "PLAYING";
		public override int StateDurationSeconds => 1200;
		[Net] public Turn Turn { get; set; }

		protected override void OnStart()
		{
			PickNextPlayer();
			(Game.Current as Game).newTerrain();
		}

		public void OnTurnFinished()
		{
			if ( CheckWinCondition() )
			{
				StateHandler.Instance?.ChangeState( new EndState() );
				return;
			}

			Crate.SpawnCrate();

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
			return StateHandler.Instance?.Players?.Count < 2;
		}

		// Debug method for changing current state to PlayingState.
		[ServerCmd( "tf_state_play" )]
		public static void PlayState()
		{
			StateHandler.Instance?.ChangeState( new PlayingState() );
		}
	}
}
