using Sandbox;
using System.Collections.Generic;

namespace TerryForm.States
{
	public partial class StateHandler : BaseNetworkable
	{
		[Net, Change] public BaseState State { get; set; } = new WaitingState();
		public List<Pawn.Player> Players { get; set; } = new();
		public static StateHandler Instance { get; set; }

		public StateHandler()
		{
			Instance = this;

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

		[Event.Tick.Server]
		private void Tick()
		{
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
