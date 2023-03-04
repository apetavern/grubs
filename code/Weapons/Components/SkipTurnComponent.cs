namespace Grubs;

[Prefab]
public partial class SkipTurnComponent : WeaponComponent
{
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
