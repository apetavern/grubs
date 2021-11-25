using Sandbox;
using Sandbox.UI;
using System;
using TerryForm.States;

namespace TerryForm.UI
{
	[UseTemplate]
	public class GameInfoPanel : Panel
	{
		public Label StateTime { get; set; }
		public Label TurnTime { get; set; }

		public GameInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/GameInfoPanel.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			var game = Game.Instance;
			if ( game == null ) return;

			var state = game.StateHandler.State;
			if ( state == null ) return;

			DebugOverlay.ScreenText( 0, state.StateName );
			DebugOverlay.ScreenText( 1, state.TimeLeft.ToString() );

			if ( state is PlayingState playingState )
			{
				DebugOverlay.ScreenText( 2, playingState.Turn.StateName );
				DebugOverlay.ScreenText( 3, playingState.Turn.TimeLeft.ToString() );

				TimeSpan stateTimeSpan = TimeSpan.FromSeconds( playingState.TimeLeft );
				StateTime.Text = string.Format( "{0:D2}:{1:D2}",
								stateTimeSpan.Minutes,
								stateTimeSpan.Seconds );

				if ( playingState.Turn == null )
				{
					TurnTime.Text = "0";
				}
				else
				{
					int timeLeftSeconds = playingState.Turn.TimeLeft.CeilToInt();
					TurnTime.Text = timeLeftSeconds.ToString();
				}
			}
			else
			{
				StateTime.Text = "0:00";
				TurnTime.Text = "0";
			}
		}
	}
}
