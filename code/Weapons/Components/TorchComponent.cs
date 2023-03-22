namespace Grubs;

[Prefab]
public partial class TorchComponent : HitScanComponent
{
	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Grub.MoveInput = -Grub.Facing * 1f;
	}
}
