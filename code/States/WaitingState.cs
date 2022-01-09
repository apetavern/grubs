using Grubs.Utils;
using Grubs.Terrain;

namespace Grubs.States
{
	public partial class WaitingState : BaseState
	{
		public override string StateName => "WAITING";

		protected override void OnStart()
		{
			Log.Info( "Generate terrain here" );

			Terrain.Terrain.Generate();

			base.OnStart();
		}

		protected override void OnFinish()
		{
			foreach ( var player in StateHandler.Instance?.Players )
				player.CreateWorms( player.Client );

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
