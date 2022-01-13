using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Grubs.UI.Elements
{
	public class Controls : Panel
	{
		bool show;

		public Controls()
		{
			StyleSheet.Load( "/UI/Elements/Controls.scss" );

			AddGlyph( "Aim", InputButton.Forward, InputButton.Back );
			AddGlyph( "Move", InputButton.Left, InputButton.Right, InputButton.Jump );
			AddGlyph( "Fire", InputButton.Attack1 );
			AddGlyph( "Inventory", InputButton.Menu );
			AddGlyph( "Toggle Controls", InputButton.Flashlight );

			BindClass( "hidden", () => show );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Input.Pressed( InputButton.Flashlight ) )
				show = !show;
		}

		private void AddGlyph( string name, params InputButton[] buttons )
		{
			var panel = Add.Panel( "control" );

			foreach ( var button in buttons )
			{
				_ = new Image()
				{
					Texture = Input.GetGlyph( button, InputGlyphSize.Small ),
					Parent = panel
				};
			}

			panel.Add.Label( name, "button-name" );
		}
	}
}
