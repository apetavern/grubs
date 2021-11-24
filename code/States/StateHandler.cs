using Sandbox;
using System.Collections.Generic;

namespace TerryForm.States
{
	public partial class StateHandler : BaseNetworkable
	{
		[Net, Change] public BaseState State { get; set; } = new WaitingState();
		public List<Pawn.Player> Players { get; set; } = new();

		public StateHandler()
		{
			State.Start();
		}

		public void OnPlayerJoin( Pawn.Player player )
		{
			Players.Add( player );

			State?.OnPlayerJoin( player );
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
			Game.Instance.State = State;
			State?.OnTick();
		}

		public void OnStateChanged( BaseState oldState, BaseState newState )
		{
			oldState?.Finish();
			oldState = newState;
			oldState.Start();
		}
	}
}
