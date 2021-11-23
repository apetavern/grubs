using Sandbox;

namespace TerryForm.States
{
	public partial class WaitingState : BaseState
	{
		public override string StateName => "WAITING";

		protected override void OnStart()
		{
			Log.Info( $"Starting {StateName} State" );
			base.OnStart();
		}

		protected override void OnFinish()
		{
			Log.Info( $"Ending {StateName} State" );
			base.OnFinish();
		}

		public override void OnPlayerJoin( Player player )
		{
			AddPlayer( player );

			base.OnPlayerJoin( player );
		}
	}
}
