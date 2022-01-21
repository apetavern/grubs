using Sandbox.UI;

namespace Grubs.UI.Menu
{
	[UseTemplate]
	public partial class CustomizePanel : Panel
	{
		public WormPreviewScene WormPreviewScene { get; set; }

		public CustomizePanel()
		{

		}
		public override void OnHotloaded()
		{
			base.OnHotloaded();

			WormPreviewScene.Build();
		}

		protected override void PostTemplateApplied()
		{
			WormPreviewScene.Build();
		}

	}
}
