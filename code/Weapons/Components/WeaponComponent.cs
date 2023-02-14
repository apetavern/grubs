namespace Grubs;

public partial class WeaponComponent : EntityComponent<Weapon>
{
	protected Weapon Weapon => Entity;
	protected Grub Grub => Weapon.Grub;
	protected Player Player => Grub.Player;

	[Net, Predicted]
	public TimeSince TimeSinceActivated { get; protected set; }

	[Net, Predicted]
	public int Charge { get; protected set; } = 0;

	[Net, Predicted]
	public bool IsFiring { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceFired { get; set; }

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

		if ( Input.Down( InputButton.PrimaryAttack ) && Weapon.FiringType is FiringType.Charged )
		{
			IncreaseCharge();
		}

		if ( Input.Released( InputButton.PrimaryAttack ) )
		{
			IsFiring = true;
		}
	}

	public void Fire()
	{
		switch ( Weapon.FiringType )
		{
			case FiringType.Instant:
				FireInstant();
				break;
			case FiringType.Charged:
				FireCharged();
				break;
			default:
				throw new NotImplementedException();
		}
	}

	public virtual void FireInstant() { }

	public virtual void FireCharged() { }

	private void IncreaseCharge()
	{
		Charge++;
		Charge = Charge.Clamp( 0, 100 );
	}
}
