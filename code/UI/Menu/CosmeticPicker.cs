using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Grubs.UI.Menu
{
	public partial class CosmeticCategory : Panel
	{
		public Label Type { get; set; }
		public IconPanel Icon { get; set; }

		public CosmeticCategory( string icon, string type )
		{
			Icon = Add.Icon( icon, "type-icon" );
			Type = Add.Label( type, "type-label" );
		}
	}

	public partial class CosmeticPicker : Panel
	{
		// Icon, name
		public List<(string, string)> CosmeticTypes = new()
		{
			("face", "Head"),
			("emoji_people", "Body"),
			("music_note", "Dance"),
			("chat_bubble", "Voice")
		};

		public List<CosmeticCategory> CosmeticCategories = new();

		public CosmeticCategory ActiveCategory { get; set; }

		public CosmeticPicker()
		{
			StyleSheet.Load( "/UI/Menu/CosmeticPicker.scss" );

			foreach ( var typePair in CosmeticTypes )
			{
				var icon = typePair.Item1;
				var type = typePair.Item2;

				var category = new CosmeticCategory( icon, type );
				CosmeticCategories.Add( category );
				AddChild( category );
			}

			ActiveCategory = CosmeticCategories[0];
			ActiveCategory.AddClass( "Active" );
			ActiveCategory.Type.AddClass( "Active" );

			DefineListeners();
		}

		private void DefineListeners()
		{
			foreach ( var category in CosmeticCategories )
			{
				category.AddEventListener( "onclick", () =>
				{
					ActiveCategory.RemoveClass( "Active" );
					ActiveCategory.Type.RemoveClass( "Active" );
					ActiveCategory = category;
					category.AddClass( "Active" );
					category.Type.AddClass( "Active" );
				} );
			}
		}
	}
}
