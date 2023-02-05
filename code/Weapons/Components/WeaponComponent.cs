namespace Grubs;

public partial class WeaponComponent : EntityComponent<Weapon>
{
	protected Weapon Weapon => Entity;
	protected Grub Grub => Weapon.Owner as Grub;
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
}
