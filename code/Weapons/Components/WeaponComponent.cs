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
	public bool IsFiring { get; set; } = false;

	[Net, Predicted]
	public bool IsCharging { get; set; } = false;

	[Net, Predicted]
	public TimeSince TimeSinceFired { get; set; }

	public Particles ChargeParticles { get; set; }

	private Sound _chargeSound;

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
			ChargeParticles?.Set( "Speed", 40f );

			if ( !_chargeSound.IsPlaying )
				_chargeSound = Weapon.PlaySound( "charge" );

			IsCharging = true;
			IncreaseCharge();
		}

		var shouldFire = Weapon.FiringType is FiringType.Charged && (Input.Released( InputAction.Fire ) || Charge >= 100);
		shouldFire |= Weapon.FiringType is not FiringType.Charged && Input.Pressed( InputAction.Fire );

		if ( shouldFire )
		{
			_chargeSound.Stop( To.Everyone );
			ChargeParticles?.Destroy( true );
			ChargeParticles = null;
			IsFiring = true;
		}
	}

	public void Fire()
	{
		Weapon.IsChargeConsumed = true;
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

		if ( Weapon.CurrentUses >= Weapon.Charges && !Weapon.CanSwapAfterUse )
			GamemodeSystem.Instance.UseTurn( true );
	}

	private void IncreaseCharge()
	{
		Charge++;
		Charge = Charge.Clamp( MinCharge, MaxCharge );
	}
}
