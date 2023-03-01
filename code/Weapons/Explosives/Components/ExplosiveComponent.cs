namespace Grubs;

public partial class ExplosiveComponent : EntityComponent<Explosive>
{
	protected Explosive Explosive => Entity;
	protected Grub Grub => Explosive.Grub;
	protected Player Player => Grub.Player;

	private bool HasStarted = false;

	public virtual void OnStart()
	{

	}

	public virtual void Simulate( IClient client )
	{
		if ( !HasStarted )
		{
			OnStart();
			HasStarted = true;
		}
	}

	public virtual void Explode()
	{

	}
}
