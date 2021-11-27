using Sandbox;
using System.Collections.Generic;

namespace TerryForm.States
{
	public partial class StateHandler : BaseNetworkable
	{
		[Net] public BaseState State { get; set; } = new WaitingState();
		public List<Pawn.Player> Players { get; set; } = new();
		public static StateHandler Instance { get; set; }

		public StateHandler()
		{
			Instance = this;

			if ( Host.IsServer )
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
		public static void Tick()
		{
			Instance?.State?.OnTick();
		}
	}
}
