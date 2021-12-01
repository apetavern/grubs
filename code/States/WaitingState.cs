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
			base.OnPlayerJoin( player );

			if ( StateHandler.Instance?.Players?.Count >= GameConfig.MinimumPlayersToStart )
			{
				StateHandler.Instance?.ChangeState( new PlayingState() );
			}
		}

		// Debug method for changing current state to WaitingState.
		[ServerCmd( "tf_state_wait" )]
		public static void WaitState()
		{
			StateHandler.Instance?.ChangeState( new WaitingState() );
		}
	}
}
