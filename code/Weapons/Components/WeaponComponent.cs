namespace Grubs;

public partial class WeaponComponent : EntityComponent<Weapon>
{
	protected Weapon Weapon => Entity;
	protected Grub Grub => Weapon.Grub;
	protected Player Player => Grub.Player;

	[Net, Predicted]
	public int Charge { get; protected set; } = MinCharge;
	public const int MinCharge = 1;
	public const int MaxCharge = 100;

	[Net, Predicted]
	public bool IsFiring { get; set; }

	[Net, Predicted]
	public bool IsCharging { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceFired { get; set; }

	public Particles ChargeParticles { get; set; }

	public virtual void OnDeploy()
	{

	}

	public virtual void OnHolster()
	{
		ChargeParticles?.Destroy( true );
		ChargeParticles = null;

		IsFiring = false;
		IsCharging = false;
	}

	public virtual void Simulate( IClient client )
	{
		if ( Weapon.CurrentUses >= Weapon.Charges || GamemodeSystem.Instance.UsedTurn )
			return;

		if ( !Grub.Controller.ShouldShowWeapon() )
			return;

		if ( Input.Down( InputAction.Fire ) && Weapon.FiringType is FiringType.Charged )
		{
			ChargeParticles ??= Particles.Create( "particles/weaponcharge/weaponcharge.vpcf" );
			ChargeParticles?.SetPosition( 0, Weapon.GetMuzzlePosition() );
			ChargeParticles?.SetPosition( 1, Weapon.GetMuzzlePosition() + Weapon.GetMuzzleForward() * 80f );
			ChargeParticles?.Set( "Alpha", 100f );
			ChargeParticles?.Set( "Speed", 50f );

			IsCharging = true;
			IncreaseCharge();
		}

		if ( Input.Released( InputAction.Fire ) || Charge == 100 )
		{
			ChargeParticles?.Set( "Alpha", 0f );
			ChargeParticles?.Set( "Speed", 10000f );
			IsFiring = true;
		}
	}

	public void Fire()
	{
		Weapon.HasFired = true;
		TimeSinceFired = 0f;
		IsCharging = false;

		switch ( Weapon.FiringType )
		{
			case FiringType.Instant:
				FireInstant();
				break;
			case FiringType.Charged:
				FireCharged();
				break;
			case FiringType.Cursor:
				FireCursor();
				break;
			default:
				throw new NotImplementedException();
		}
	}

	public virtual void FireInstant() { }

	public virtual void FireCharged() { }

	public virtual void FireCursor() { }

	public virtual void FireFinished()
	{
		IsFiring = false;
		Weapon.CurrentUses++;

		if ( Weapon.CurrentUses >= Weapon.Charges )
			GamemodeSystem.Instance.UseTurn( true );
	}

	private void IncreaseCharge()
	{
		Charge++;
		Charge = Charge.Clamp( MinCharge, MaxCharge );
	}
}
