using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using TerryForm.States.SubStates;

namespace TerryForm.UI
{
	public class WindInfoPanel : Panel
	{
		public static WindInfoPanel Instance { get; set; }

		public WindInfoPanel()
		{
			StyleSheet.Load( "/Code/UI/WindInfoPanel.scss" );
			Instance = this;
		}

		public void UpdateWind( float wind )
		{
			DeleteChildren( true );

			var direction = wind < 0 ? "left" : "right";
			var segmentQuantity = Math.Abs( wind ) * 10;

			for ( int i = 1; i < segmentQuantity; i++ )
			{
				Add.Icon( $"arrow_{direction}" );
			}
		}
	}
}
