namespace Grubs;

public partial class GadgetComponent : EntityComponent<Gadget>
{
	protected Gadget Gadget => Entity;
	protected Grub Grub => Gadget.Grub;
	protected Player Player => Grub.Player;

	[Prefab, Net]
	public int SortOrder { get; set; } = 0;

	public virtual void Spawn()
	{

	}

	public virtual void ClientSpawn()
	{

	}

	public virtual bool IsResolved()
	{
		return true;
	}

	public virtual void OnUse( Weapon weapon, int charge )
	{

	}

	public virtual void Touch( Entity other )
	{

	}

	public virtual void Simulate( IClient client )
	{

	}
}
