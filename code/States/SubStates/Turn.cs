using Sandbox;
using TerryForm.Utils;
using TerryForm.UI.Elements;

namespace TerryForm.States.SubStates
{
	public partial class Turn : BaseState
	{
		public override string StateName => "TURN";
		public override int StateDurationSeconds => GameConfig.TurnDurationSeconds;
		public static Turn Instance { get; set; }
		private PlayingState PlayingState { get; set; }
		[Net] public Pawn.Player ActivePlayer { get; set; }
		[Net] public Vector3 WindForce { get; set; }

		public Turn()
		{
			Instance = this;
		}

		public Turn InitFrom( Pawn.Player player, PlayingState playState )
		{
			ActivePlayer = player;
			PlayingState = playState;

			return this;
		}

		protected override void OnStart()
		{
			base.OnStart();

			WindForce = Vector3.Random.WithY( 0 ).WithZ( 0 ) / 4;

			// Let the player know that their turn has started.
			ActivePlayer?.OnTurnStart();

			ChatBox.AddInformation( To.Everyone, $"{ActivePlayer.ActiveWorm.Name}'s turn has started.", $"avatar:{ActivePlayer.Client.PlayerId}" );

			// Update camera target for all players.
			StateHandler.Instance?.Players?.ForEach( player => player.UpdateCameraTarget( ActivePlayer.ActiveWorm ) );
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

		public void ForceEnd()
		{
			OnFinish();
		}

		// Debug method for ending a turn immediately.
		[ServerCmd( "tf_turn_end" )]
		public static void EndTurn()
		{
			(StateHandler.Instance?.State as PlayingState)?.Turn?.OnFinish();
		}
	}
}
