using Grubs.Gamemodes;
using Grubs.Helpers;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Weapon" ), Category( "Equipment" )]
public partial class Weapon : Component
{
	public delegate void OnFireDelegate( int charge );

	[Property] public required Equipment Equipment { get; set; }

	[Property] public float Cooldown { get; set; } = 2f;
	[Property] public bool CanFireWhileMoving { get; set; } = false;
	[Property] public bool CanSwapAfterUse { get; set; } = false;
	[Property] public bool CanSwapDuringUse { get; set; } = false;
	[Property] public int MaxUses { get; set; } = 1;
	[Property] public FiringType FiringType { get; set; } = FiringType.Instant;
	[Property] public OnFireDelegate OnFire { get; set; }

	public bool IsFiring { get; set; }
	public bool IsCharging { get; set; }
	public bool ForceHideWeapon { get; set; }
	public TimeSince TimeSinceLastUsed { get; set; }
	public int TimesUsed { get; set; }

	private int _weaponCharge;
	private SceneParticles _chargeParticles;
	private ParticleSystem ChargeParticleSystem { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		TimeSinceLastUsed = Cooldown;
		ChargeParticleSystem = ParticleSystem.Load( "particles/weaponcharge/weaponcharge.vpcf" );
	}

	protected override void OnUpdate()
	{
		if ( Equipment.Grub is not { } grub )
			return;

		if ( IsProxy || !Equipment.Deployed )
			return;

		if ( TimeSinceLastUsed < Cooldown )
			return;

		if ( !CanFireWhileMoving && !grub.PlayerController.Velocity.IsNearlyZero( 0.002f ) )
			return;

		if ( TimesUsed >= MaxUses )
			return;

		if ( FiringType is FiringType.Charged )
		{
			if ( Input.Down( "fire" ) )
			{
				OnChargedHeld();
				return;
			}

			if ( Input.Released( "fire" ) )
			{
				IsFiring = true;
				IsCharging = false;
				ParticleHelper.Instance.Dispose( _chargeParticles );
				_chargeParticles = ParticleHelper.Instance.PlayInstantaneous( ChargeParticleSystem );

				if ( OnFire is not null )
					OnFire.Invoke( _weaponCharge );
				else
					FireCharged( _weaponCharge );

				TimeSinceLastUsed = 0;
				_weaponCharge = 0;

				FireFinished();
			}
		}
		else if ( FiringType is FiringType.Instant )
		{
			if ( Input.Pressed( "fire" ) )
			{
				IsFiring = true;

				if ( OnFire is not null )
					OnFire.Invoke( 100 );
				else
					FireImmediate();

				TimeSinceLastUsed = 0;
			}
		}
		else if ( FiringType is FiringType.Cursor )
		{
			if ( Input.Pressed( "fire" ) )
			{
				IsFiring = true;

				if ( OnFire is not null )
					OnFire.Invoke( 100 );
				else
					FireImmediate();
				TimeSinceLastUsed = 0;
				FireFinished();
			}
		}
		else if ( FiringType is FiringType.Complex )
		{
			HandleComplexFiringInput();
		}
		else if ( FiringType is FiringType.Continuous )
		{
			if ( Input.Down( "fire" ) && TimesUsed < MaxUses && TimeSinceLastUsed > Cooldown )
			{
				IsFiring = true;

				if ( OnFire is not null )
					OnFire.Invoke( 100 );
				else
					FireImmediate();

				TimeSinceLastUsed = 0;
				FireFinished();
			}
		}
	}

	protected virtual void FireImmediate() { }
	protected virtual void FireCharged( int charge ) { }
	protected virtual void HandleComplexFiringInput() { }

	protected virtual void FireFinished()
	{
		IsFiring = false;
		TimesUsed++;

		if ( TimesUsed >= MaxUses )
		{
			Equipment.UseAmmo();

			if ( Equipment.Grub is not { } grub )
				return;

			if ( !CanSwapAfterUse )
			{
				grub.Player.HasFiredThisTurn = true;
				using ( Rpc.FilterInclude( c => c.IsHost ) )
					Gamemode.FFA.UseTurn( true );
			}
			else
			{
				grub.Player.Inventory.Holster( grub.Player.Inventory.ActiveSlot );
			}
		}
	}

	protected void OnChargedHeld()
	{
		IsCharging = true;

		var muzzle = GetMuzzlePosition();
		_chargeParticles ??= ParticleHelper.Instance.PlayInstantaneous( ChargeParticleSystem, muzzle );
		_chargeParticles?.SetControlPoint( 0, muzzle.Position );
		_chargeParticles?.SetControlPoint( 1, muzzle.Position + GetMuzzleForward() * 80f );
		_chargeParticles?.SetNamedValue( "Alpha", 100f );
		_chargeParticles?.SetNamedValue( "Speed", 40f );

		_weaponCharge++;
		_weaponCharge.Clamp( 0, 100 );
	}

	public Vector3 GetStartPosition( bool isDroppable = false )
	{
		if ( FiringType is FiringType.Cursor )
			return Vector3.Zero;

		if ( Equipment.Grub is not { } grub )
			return Vector3.Zero;

		var controller = grub.CharacterController;

		if ( isDroppable )
		{
			// Perform a forward trace to find the position to drop the item
			var dropTr = Scene.Trace.Ray( grub.EyePosition.Position, grub.EyePosition.Position + grub.Transform.Rotation.Forward * 25f )
				.IgnoreGameObjectHierarchy( grub.GameObject )
				.IgnoreGameObject( GameObject )
				.Radius( 1f )
				.Run();

			var startPosition = dropTr.EndPosition;

			if ( dropTr.Hit )
			{
				startPosition -= grub.Transform.Rotation.Forward * 10f;
			}

			return startPosition.WithY( 512 );
		}

		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return grub.EyePosition.Position;

		var tr = Scene.Trace.Ray( controller.BoundingBox.Center + grub.Transform.Position, muzzle.Value.Position )
			.IgnoreGameObjectHierarchy( grub.GameObject )
			.WithoutTags( "projectile" )
			.Radius( 1f )
			.Run();

		return tr.EndPosition.WithY( 512 );
	}

	protected Transform GetMuzzlePosition()
	{
		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		return muzzle ?? Equipment.Grub.EyePosition;
	}

	protected Vector3 GetMuzzleForward()
	{
		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Equipment.Grub.PlayerController.EyeRotation.Forward * Equipment.Grub.PlayerController.Facing;
		return muzzle.Value.Rotation.Forward;
	}
}
