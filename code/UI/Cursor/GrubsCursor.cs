namespace Grubs.UI;

public partial class GrubsCursor : Panel
{
	public override void Tick()
	{
		var mousePosition = Mouse.Position / Screen.Size;

		Style.Left = Length.Fraction( mousePosition.x );
		Style.Top = Length.Fraction( mousePosition.y );
	}
}
