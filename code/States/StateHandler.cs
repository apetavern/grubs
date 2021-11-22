using Sandbox;

namespace TerryForm.States
{
	public partial class StateHandler : BaseNetworkable
	{
		[Net] public BaseState State { get; set; } = new WaitingState();

		public StateHandler()
		{
			State.Start();
		}

		public void ChangeState( BaseState state )
		{
			Assert.NotNull( state );

			State?.Finish();
			State = state;
			State?.Start();
		}

		[Event.Tick]
		private void Tick()
		{
			State?.OnTick();
		}
	}
}
