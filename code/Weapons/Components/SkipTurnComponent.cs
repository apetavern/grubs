namespace Grubs;

[Prefab]
public partial class SkipTurnComponent : WeaponComponent
{
	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireInstant()
	{
		IsFiring = false;
		FireFinished();
	}
}
