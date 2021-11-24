using Sandbox;
using TerryForm.Utils;

namespace TerryForm.States.SubStates
{
	public partial class Turn : BaseState
	{
		public override string StateName => "TURN";
		public override int StateDurationSeconds => GameConfig.TurnDurationSeconds;
		public Pawn.Player ActivePlayer { get; set; }
		public static Turn Instance { get; set; }

		public Turn( Pawn.Player player )
		{
			ActivePlayer = player;
			Instance = this;
		}

		protected override void OnStart()
		{
			base.OnStart();
			AssignPawn();
			ActivePlayer.OnTurnStart();
		}

		protected override void OnFinish()
		{
			base.OnFinish();

			ActivePlayer.OnTurnEnd();
			RotatePlayers();
		}

		[ClientRpc]
		public static void AssignPawn()
		{
			var stateHandler = Game.StateHandler;
			if ( stateHandler == null ) return;

			if ( stateHandler.State is PlayingState )
			{
				Local.Client.Pawn = (stateHandler.State as PlayingState).Turn.ActivePlayer.ActiveWorm;
			}
		}
	}
}
