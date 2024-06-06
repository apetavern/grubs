using Grubs.Gamemodes;
using Grubs.Helpers;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Weapon" ), Category( "Equipment" )]
public partial class Weapon : Component
{
	public delegate void OnFireDelegate( int charge );
	public delegate void OnFireFinishedDelegate();

	[Property] public required Equipment Equipment { get; set; }

	[Property] public float Cooldown { get; set; } = 2f;
	[Property] public bool CanFireWhileMoving { get; set; } = false;
	[Property] public bool CanSwapAfterUse { get; set; } = false;
	[Property] public bool CanSwapDuringUse { get; set; } = false;
	[Property] public int MaxUses { get; set; } = 1;
	[Property] public FiringType FiringType { get; set; } = FiringType.Instant;
	[Property] public OnFireDelegate OnFire { get; set; }
	[Property] public OnFireFinishedDelegate OnFireFinished { get; set; }

	public bool IsFiring { get; set; }
	public bool IsCharging { get; set; }
	public bool ForceHideWeapon { get; set; }
	public TimeSince TimeSinceLastUsed { get; set; }
	public int TimesUsed { get; set; }

	private int _weaponCharge;
	private SceneParticles _chargeParticles;
	private ParticleSystem ChargeParticleSystem { get; set; }
	private SoundHandle ChargeSound { get; set; }

	private SkinnedModelRenderer ChargeGuage { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		TimeSinceLastUsed = Cooldown;

		Sound.Preload( "charge" );
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
				if ( _weaponCharge < 100 )
					return;
			}

			if ( Input.Released( "fire" ) || _weaponCharge >= 100 )
			{
				IsFiring = true;
				IsCharging = false;
				ChargeSound?.Stop();

				ChargeGuage.GameObject.Enabled = false;

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

				if ( Input.UsingController ) Input.TriggerHaptics( 0, 0.25f, rightTrigger: 0.25f );
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

				if ( Input.UsingController ) Input.TriggerHaptics( 0, 0.25f, rightTrigger: 0.25f );
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

				if ( Input.UsingController ) Input.TriggerHaptics( 0, 0.25f, rightTrigger: 0.25f );

				TimeSinceLastUsed = 0;
				FireFinished();
			}
		}
	}

	protected virtual void FireImmediate() { }
	protected virtual void FireCharged( int charge ) { }
	protected virtual void HandleComplexFiringInput() { }
	public virtual void OnHolster() { }

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

		OnFireFinished?.Invoke();
	}

	private TimeSince _startedCharging;
	protected void OnChargedHeld()
	{
		if ( !IsCharging )
			_startedCharging = 0f;

		if ( ChargeSound == null || !ChargeSound.IsPlaying )
			ChargeSound = Sound.Play( "charge" );

		if ( !ChargeGuage.IsValid() )
		{
			ChargeGuage = new GameObject().Components.Create<SkinnedModelRenderer>();
			ChargeGuage.Model = Model.Load( "particles/weaponcharge/weapon_charge.vmdl" );
		}
		ChargeGuage.GameObject.Enabled = true;

		IsCharging = true;

		var muzzle = GetMuzzlePosition();

		ChargeGuage.Transform.Position = muzzle.Position;
		ChargeGuage.Transform.Rotation = muzzle.Rotation * Rotation.FromPitch( 90 );
		ChargeGuage.SceneModel.Attributes.Set( "charge", _weaponCharge / 100f );

		_weaponCharge = (int)Math.Clamp( _startedCharging / 2f * 100f, 0f, 100f );

		if ( Input.UsingController ) Input.TriggerHaptics( 0, _weaponCharge / 100f, rightTrigger: _weaponCharge / 100f );
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
			var dropTr = Scene.Trace.Ray( grub.EyePosition.Position,
					grub.EyePosition.Position + grub.Transform.Rotation.Forward * 25f )
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

	public Transform GetMuzzlePosition()
	{
		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		return muzzle ?? Equipment.Grub.EyePosition;
	}

	public Vector3 GetMuzzleForward()
	{
		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return Equipment.Grub.PlayerController.EyeRotation.Forward * Equipment.Grub.PlayerController.Facing;
		return muzzle.Value.Rotation.Forward;
	}
}
