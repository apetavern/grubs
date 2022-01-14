using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Grubs.UI.Elements
{
	public class Controls : Panel
	{
		bool show;

		List<Glyph> glyphs;

		public Controls()
		{
			StyleSheet.Load( "/UI/Elements/Controls.scss" );

			glyphs = new();

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

			if ( !IsVisible )
				return;

			foreach ( var glyph in glyphs )
			{
				glyph.Tick();
			}
		}

		private void AddGlyph( string name, params InputButton[] buttons )
		{
			var panel = Add.Panel( "control" );

			foreach ( var button in buttons )
			{
				var glyph = new Glyph( button );
				glyph.Parent = panel;
				glyphs.Add( glyph );
			}

			panel.Add.Label( name, "button-name" );
		}

		class Glyph : Panel
		{
			Image image;
			InputButton button;

			public Glyph( InputButton button )
			{
				this.button = button;

				image = new Image()
				{
					Texture = Input.GetGlyph( button, InputGlyphSize.Small ),
					Parent = this
				};
			}

			public override void Tick()
			{
				base.Tick();

				image.Texture = Input.GetGlyph( button, InputGlyphSize.Small );
				if ( image.Texture != null )
				{
					image.Style.Width = image.Texture.Width;
					image.Style.Height = image.Texture.Height;
				}
			}
		}
	}
}
