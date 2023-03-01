namespace Grubs;

public partial class ExplosiveComponent : EntityComponent<Explosive>
{
	protected Explosive Explosive => Entity;
	protected Grub Grub => Explosive.Grub;
	protected Player Player => Grub.Player;

	public virtual void Simulate( IClient client )
	{

	}

	public virtual void Explode()
	{

	}
}
