namespace Grubs;

public partial class GadgetComponent : EntityComponent<Gadget>
{
	protected Gadget Gadget => Entity;
	protected Grub Grub => Gadget.Grub;
	protected Player Player => Grub.Player;

	public virtual void OnClientSpawn()
	{

	}

	public virtual void OnUse( Weapon weapon, int charge )
	{

	}

	public virtual void Simulate( IClient client )
	{

	}
}
