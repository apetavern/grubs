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
			if ( Host.IsServer )
			{
				var stateHandler = Game.StateHandler;

				Turn = new Turn( stateHandler.Players[0] );
				Turn?.Start();

				base.OnStart();
			}

		}

		public override void OnPlayerJoin( Pawn.Player player )
		{
			base.OnPlayerJoin( player );
		}

		public void ChangeTurn()
		{
			Turn?.Finish();
			Turn = new Turn( Game.StateHandler.Players[0] );
			Turn?.Start();

			Log.Info( Turn.ActivePlayer.Name );
		}
	}
}
