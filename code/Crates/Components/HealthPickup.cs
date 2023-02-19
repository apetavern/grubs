namespace Grubs;

[Prefab]
public partial class HealthPickup : PickupComponent
{
	public override void OnPickup( Grub grub )
	{
		grub.GiveHealth( 50 );
	}
}
