using Grubs.Pawn;

namespace Grubs.States
{
	public partial class EndState : BaseState
	{
		public override string StateName => "END";
		public override int StateDurationSeconds => 60;

		protected override void OnStart()
		{
			base.OnStart();
		}

		protected override void OnFinish()
		{
			base.OnFinish();
		}

		public override void OnPlayerJoin( Player player )
		{
			base.OnPlayerJoin( player );
		}

		// Debug method for changing current state to EndState.
		[ServerCmd( "tf_state_end" )]
		public static void EndGame()
		{
			StateHandler.Instance?.ChangeState( new EndState() );
		}
	}
}
