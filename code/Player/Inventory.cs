namespace Grubs;

public partial class Inventory : EntityComponent<Player>
{
	[Net]
	public IList<Weapon> Weapons { get; private set; }

	public Weapon ActiveWeapon { get; private set; }

	public virtual void Simulate( IClient client )
	{

	}
}
