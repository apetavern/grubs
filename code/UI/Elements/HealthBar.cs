using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TerryForm.UI.Elements
{
	public class HealthBar : Panel
	{
		Label healthLabel;

		public HealthBar()
		{
			StyleSheet.Load( "/Code/UI/Elements/HealthBar.scss" );

			Add.Panel( "health-bar-inner" );
			Add.Label( "50%", "health-label" );
		}

		public void SetValue()
		{

		}
	}
}
