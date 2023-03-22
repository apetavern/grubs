namespace Grubs;

public partial class GadgetComponent : EntityComponent<Gadget>
{
	protected Gadget Gadget => Entity;
	protected Grub Grub => Gadget.Grub;
	protected Player Player => Grub.Player;

	public virtual void Spawn()
	{

	}

	public virtual void OnClientSpawn()
	{

	}

	public virtual bool IsResolved()
	{
		return false;
	}

	public virtual void OnUse( Weapon weapon, int charge )
	{

	}

	public virtual void OnTouch( Entity other )
	{

	}

	public virtual void Simulate( IClient client )
	{

	}
}
