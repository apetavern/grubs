using Sandbox;
using System.Collections.Generic;

namespace TerryForm.States.SubStates
{
	public partial class Turn : BaseState
	{
		public override string StateName => "TURN";
		public override int StateDuration => 45;
		public Pawn.Player ActivePlayer { get; set; }

		public Turn( Pawn.Player player )
		{
			ActivePlayer = player;
		}

		protected override void OnStart()
		{
			base.OnStart();

			ActivePlayer.OnTurnStart();
		}

		protected override void OnFinish()
		{
			base.OnFinish();

			ActivePlayer.OnTurnEnd();
			RotatePlayers();
		}
	}
}
