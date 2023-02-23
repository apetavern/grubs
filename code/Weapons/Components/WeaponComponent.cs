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

	public Particles ChargeParticles { get; set; }

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
			ChargeParticles ??= Particles.Create( "particles/weaponcharge/weaponcharge.vpcf" );
			ChargeParticles?.Set( "Alpha", 100f );
			ChargeParticles?.SetPosition( 0, GetMuzzlePosition() );
			ChargeParticles?.SetPosition( 1, GetMuzzlePosition() + GetMuzzleRotation().Forward * 80f );
			ChargeParticles?.Set( "Speed", 50f );
			IncreaseCharge();
		}

		if ( Input.Released( InputButton.PrimaryAttack ) )
		{
			TimeSinceFired = 0f;
			ChargeParticles?.Set( "Alpha", 0f );
			//ChargeParticles?.Destroy();
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

	public Vector3 GetMuzzlePosition()
	{
		var muzzle = Weapon.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyePosition;
		return muzzle.Value.Position;
	}

	public Rotation GetMuzzleRotation()
	{
		var muzzle = Weapon.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Grub.EyeRotation;
		return muzzle.Value.Rotation;
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
		Log.Info( Charge );
		Charge++;
		Charge = Charge.Clamp( 0, 100 );
	}
}
