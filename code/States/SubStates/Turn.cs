using TerryForm.Utils;

namespace TerryForm.States.SubStates
{
	public partial class Turn : BaseState
	{
		public override string StateName => "TURN";
		public override int StateDurationSeconds => GameConfig.TurnDurationSeconds;
		public static Turn Instance { get; set; }
		private PlayingState PlayingState { get; set; }
		public Pawn.Player ActivePlayer { get; set; }

		public Turn( Pawn.Player player, PlayingState playState )
		{
			Instance = this;

			ActivePlayer = player;
			PlayingState = playState;
		}

		protected override void OnStart()
		{
			base.OnStart();

			// Let the player know that their turn has started.
			ActivePlayer?.OnTurnStart();
		}

		protected override void OnTimeUp()
		{
			OnFinish();
		}

		protected override void OnFinish()
		{
			base.OnFinish();

			// Let the player know that their turn has ended, useful to kill their ActiveWorm.
			ActivePlayer?.OnTurnEnd();

			// Let the playing state know this turn has ended so that it can start another.
			PlayingState?.OnTurnFinished();
		}

		// Debug method for ending a turn immediately.
		[ServerCmd]
		public static void EndTurn()
		{
			(StateHandler.Instance?.State as PlayingState)?.Turn?.OnFinish();
		}
	}
}
