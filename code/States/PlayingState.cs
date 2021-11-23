using Sandbox;
using System.Linq;
using TerryForm.States.SubStates;

namespace TerryForm.States
{
	public partial class PlayingState : BaseState
	{
		public override string StateName => "PLAYING";
		public override int StateDuration => 1200;
		public Turn Turn { get; set; }

		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				Turn = new Turn( new Pawn.Player() );
				Turn?.Start();

				base.OnStart();
			}

		}

		public override void OnPlayerJoin( Pawn.Player player )
		{
			base.OnPlayerJoin( player );
		}

		public void ChangeTurn()
		{
			Turn?.Finish();
			Turn = new Turn( PlayerList[0] );
			Turn?.Start();

			Log.Info( Turn.ActivePlayer.Name );
		}
	}
}
