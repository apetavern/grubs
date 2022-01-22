using Grubs.Utils;

namespace Grubs.States
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
			// For now, generate terrain when we are ending WaitingState.
			Terrain.Terrain.Generate();

			foreach ( var player in StateHandler.Instance?.Players )
				player.CreateWorms( player.Client );

			base.OnFinish();
		}

		public override void OnPlayerJoin( Pawn.Player player )
		{
			base.OnPlayerJoin( player );
		}

		// Debug method for changing current state to WaitingState.
		[ServerCmd( "tf_state_wait" )]
		public static void WaitState()
		{
			StateHandler.Instance?.ChangeState( new WaitingState() );
		}
	}
}
