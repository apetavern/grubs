namespace Grubs;

public partial class WeaponComponent : EntityComponent<Weapon>
{
	protected Weapon Weapon => Entity;
	protected Grub Grub => Weapon.Grub;
	protected Player Player => Grub.Player;

	[Net, Predicted]
	public TimeSince TimeSinceActivated { get; protected set; }

	[Net, Predicted]
	public int Charge { get; protected set; } = MinCharge;
	protected const int MaxCharge = 100;
	protected const int MinCharge = 1;

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

		if ( Weapon.CurrentUses >= Weapon.Charges || GamemodeSystem.Instance.UsedTurn )
			return;

		if ( !Grub.Controller.ShouldShowWeapon() )
			return;

		if ( Input.Down( InputButton.PrimaryAttack ) && Weapon.FiringType is FiringType.Charged )
		{
			IncreaseCharge();
		}

		if ( Input.Released( InputButton.PrimaryAttack ) )
		{
			TimeSinceFired = 0f;
			IsFiring = true;
			FireStart();
			Weapon.CurrentUses++;
		}
	}

	public void Fire()
	{
		Weapon.HasFired = true;

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

	public virtual void FireStart() { }

	public virtual void FireFinished()
	{
		if ( Weapon.CurrentUses >= Weapon.Charges )
			GamemodeSystem.Instance.UseTurn( true );
	}

	private void IncreaseCharge()
	{
		Charge++;
		Charge = Charge.Clamp( MinCharge, MaxCharge );
	}
}
