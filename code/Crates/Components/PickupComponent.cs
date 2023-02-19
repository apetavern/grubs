namespace Grubs;

public partial class PickupComponent : EntityComponent<Crate>
{
	protected Crate Crate => Entity;
	public virtual void OnPickup( Grub grub ) { }
}
