namespace Grubs;

public class GrubsHud : RootPanel
{
	private AimReticle _aimReticle;

	public GrubsHud()
	{
		Game.AssertClient();

		_aimReticle = new AimReticle();
	}
}
