namespace Grubs;

public partial class ExplosiveComponent : EntityComponent<Explosive>
{
	protected Explosive Explosive => Entity;
	protected Grub Grub => Explosive.Grub;
	protected Player Player => Grub.Player;

	[Net, Predicted]
	public TimeSince TimeSinceActivated { get; protected set; }

	public virtual bool ShouldStart()
	{
		return false;
	}

	public virtual void OnStart()
	{
		TimeSinceActivated = 0;
	}

	public virtual void Simulate( IClient client )
	{
		if ( ShouldStart() )
			OnStart();
	}

	public virtual void Explode()
	{

	}
}
