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
			if ( game == null )
				return;

			var state = game.StateHandler.State;
			if ( state == null )
				return;

			if ( state is PlayingState playingState )
			{
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
					TurnTime.Text = timeLeftSeconds <= 0 ? "-" : timeLeftSeconds.ToString();
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
