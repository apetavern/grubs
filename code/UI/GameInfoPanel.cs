using Sandbox.UI;
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

			if ( Game.StateHandler.State is PlayingState playingState )
			{
				StateTime.Text = playingState.TimeLeft.ToString();
				TurnTime.Text = playingState.Turn.TimeLeft.ToString();
			}
		}
	}
}
