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
	}

	public virtual void Simulate( IClient client )
	{
		if ( Weapon.CurrentUses >= Weapon.Charges || GamemodeSystem.Instance.UsedTurn )
			return;

		if ( !Grub.Controller.ShouldShowWeapon() )
			return;

		if ( Input.Down( InputButton.PrimaryAttack ) && Weapon.FiringType is FiringType.Charged )
		{
			ChargeParticles ??= Particles.Create( "particles/weaponcharge/weaponcharge.vpcf" );
			ChargeParticles?.SetPosition( 0, GetMuzzlePosition() );
			ChargeParticles?.SetPosition( 1, GetMuzzlePosition() + GetMuzzleForward() * 80f );
			ChargeParticles?.Set( "Alpha", 100f );
			ChargeParticles?.Set( "Speed", 50f );

			IsCharging = true;
			IncreaseCharge();
		}

		if ( Input.Released( InputButton.PrimaryAttack ) || Charge == 100 )
		{
			ChargeParticles?.Set( "Alpha", 0f );
			ChargeParticles?.Set( "Speed", 10000f );

			TimeSinceFired = 0f;
			IsFiring = true;
			IsCharging = false;
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
			case FiringType.Cursor:
				FireCursor();
				break;
			default:
				throw new NotImplementedException();
		}
	}

	public Vector3 GetMuzzlePosition()
	{
		var muzzle = Weapon.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyePosition;
		return muzzle.Value.Position;
	}

	public Vector3 GetMuzzleForward()
	{
		var muzzle = Weapon.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyeRotation.Forward * Grub.Facing;
		return muzzle.Value.Rotation.Forward;
	}

	public virtual void FireInstant() { }

	public virtual void FireCharged() { }

	public virtual void FireCursor() { }

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
