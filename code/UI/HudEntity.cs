using Sandbox.UI;
using Sandbox.UI.Construct;
using TerryForm.States;
using TerryForm.UI.World;

namespace TerryForm.UI
{
	public class HudEntity : Sandbox.HudEntity<RootPanel>
	{
		public static HudEntity Instance { get; set; }
		public StateEntitySwitcher StateSwitcher { get; set; }
		private bool ShouldReceiveInput { get; set; } = false;

		public HudEntity()
		{
			if ( IsClient )
			{
				StateSwitcher = new();

				Instance = this;
				RootPanel.SetTemplate( "/Code/UI/HudEntity.html" );

				_ = new WormNametags();
			}
		}

		public void ReceiveInput( bool receiveInput )
		{
			ShouldReceiveInput = receiveInput;
			RootPanel.SetClass( "ReceiveInput", ShouldReceiveInput );
		}
	}

	public class StateEntitySwitcher : Panel
	{
		public Panel ActivePanel { get; set; }

		public StateEntitySwitcher()
		{
			ActivePanel = new WaitingEntity();
		}

		public override void Tick()
		{
			var stateHandler = Game.Instance?.StateHandler;
			if ( stateHandler == null ) return;

			var state = stateHandler.State;
			if ( state == null ) return;

			if ( state is WaitingState && ActivePanel is not WaitingEntity )
			{
				ActivePanel.Delete();
				ActivePanel = AddChild<WaitingEntity>();
			}
			else if ( state is PlayingState && ActivePanel is not PlayingEntity )
			{
				ActivePanel.Delete();
				ActivePanel = AddChild<PlayingEntity>();
			}

			base.Tick();
		}
	}
}
