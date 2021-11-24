using TerryForm.Utils;

namespace TerryForm.States
{
	public partial class WaitingState : BaseState
	{
		public override string StateName => "WAITING";

		protected override void OnStart()
		{
			base.OnStart();
		}

		protected override void OnFinish()
		{
			base.OnFinish();
		}

		public override void OnPlayerJoin( Pawn.Player player )
		{
			AddPlayer( player );

			if ( PlayerList.Count >= GameConfig.MinimumPlayersToStart )
			{
				Game.StateHandler.ChangeState( new PlayingState() );
			}
		}

		// Debug method for changing current state to WaitingState.
		[ServerCmd]
		public static void WaitState()
		{
			Game.StateHandler.ChangeState( new WaitingState() );
		}
	}
}
