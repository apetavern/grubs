using Sandbox;
using TerryForm.Pawn;
using TerryForm.Utils;

namespace TerryForm.States.SubStates
{
	public partial class Turn : BaseState
	{
		public override string StateName => "TURN";
		public override int StateDurationSeconds => GameConfig.TurnDurationSeconds;
		public Pawn.Player ActivePlayer { get; set; }
		public static Turn Instance { get; set; }
		private PlayingState PlayingState { get; set; }

		public Turn( Pawn.Player player, PlayingState playState )
		{
			ActivePlayer = player;
			Instance = this;
			PlayingState = playState;
		}

		protected override void OnStart()
		{
			base.OnStart();

			Log.Info( $"Starting turn for {ActivePlayer}" );

			// Let the player know that their turn has started.
			ActivePlayer?.OnTurnStart();
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
			(Game.StateHandler.State as PlayingState)?.Turn?.OnFinish();
		}
	}
}
