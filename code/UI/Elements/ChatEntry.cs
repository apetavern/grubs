using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Grubs.UI.Elements
{
	public partial class ChatEntry : Panel
	{
		public Label NameLabel { get; internal set; }
		public Label Message { get; internal set; }
		public Image Avatar { get; internal set; }

		private RealTimeSince TimeSinceBorn = 0;

		public ChatEntry()
		{
			Avatar = Add.Image();
			NameLabel = Add.Label( "Name", "name" );
			Message = Add.Label( "Message", "message" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( TimeSinceBorn > 3 && !ChatBox.Current.HasClass( "open" ) )
			{
				Hide();
			}
		}

		public void Hide()
		{
			AddClass( "hide" );
		}
	}
}
