using Sandbox;
using TerryForm.States.SubStates;

namespace TerryForm.States
{
	public partial class PlayingState : BaseState
	{
		public override string StateName => "PLAYING";
		public override int StateDurationSeconds => 1200;
		public Turn Turn { get; set; }

		protected override void OnStart()
		{
			PickNextPlayer();
		}

		public void OnTurnFinished()
		{
			PickNextPlayer();
		}

		protected void PickNextPlayer()
		{
			RotatePlayers();

			Turn = new Turn( Game.StateHandler?.Players[0], this );
			Turn?.Start();

			Log.Info( $"Next player is {Game.StateHandler.Players[0].ClientOwner.Name}" );
		}

		// Debug method for changing current state to PlayingState.
		[ServerCmd]
		public static void PlayState()
		{
			Game.StateHandler.ChangeState( new PlayingState() );
		}
	}
}
