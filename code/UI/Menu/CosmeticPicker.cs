using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Grubs.UI.Menu
{
	public partial class CosmeticCategory : Panel
	{
		public Label Type { get; set; }

		public CosmeticCategory( string type )
		{
			Type = Add.Label( type, "type-label" );
		}
	}

	public partial class CosmeticPicker : Panel
	{
		public List<string> CosmeticTypes = new()
		{
			"head",
			"body",
			"dance",
			"voice"
		};

		public List<CosmeticCategory> CosmeticCategories = new();

		public CosmeticCategory ActiveCategory { get; set; }

		public CosmeticPicker()
		{
			StyleSheet.Load( "/UI/Menu/CosmeticPicker.scss" );

			foreach ( var type in CosmeticTypes )
			{
				var category = new CosmeticCategory( type );
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
