using Grubs.Gamemodes;
using Grubs.Pawn;
using Grubs.UI;

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
	[Property] public int StartMultiplier { get; set; } = 1;
	[Property] public FiringType FiringType { get; set; } = FiringType.Instant;
	[Property] public AmmoType AmmoType { get; set; } = AmmoType.Numbered;
	[Property] public SoundEvent UseSound { get; set; }
	[Property] public OnFireDelegate OnFire { get; set; }
	[Property] public OnFireFinishedDelegate OnFireFinished { get; set; }

	public bool IsFiring { get; set; }
	public bool IsCharging { get; set; }
	public bool ForceHideWeapon { get; set; }
	public TimeSince TimeSinceLastUsed { get; set; }
	public float TimesUsed { get; set; }

	protected WeaponInfo WeaponInfoPanel;

	private int _weaponCharge;
	private SoundHandle ChargeSound { get; set; }

	private SkinnedModelRenderer ChargeGauge { get; set; }

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

				FireEffects();

				if ( ChargeGauge.IsValid() && ChargeGauge.GameObject.IsValid() )
					ChargeGauge.GameObject.Enabled = false;

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

				FireEffects();

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

				FireEffects();

				if ( OnFire is not null )
					OnFire.Invoke( 100 );
				else
					FireImmediate();

				if ( Input.UsingController ) Input.TriggerHaptics( 0, 0.25f, rightTrigger: 0.25f );

				TimeSinceLastUsed = 0;
				if ( StartMultiplier > 1 && Input.Pressed( "fire" ) )
					TimesUsed += StartMultiplier - 1;

				FireFinished();
			}
		}
	}

	protected virtual void FireImmediate() { }
	protected virtual void FireCharged( int charge ) { }
	protected virtual void HandleComplexFiringInput() { }

	public virtual void OnDeploy()
	{
		if ( IsProxy )
			return;

		var prefab = ResourceLibrary.Get<PrefabFile>( "prefabs/world/weaponinfo.prefab" );
		var panel = SceneUtility.GetPrefabScene( prefab ).Clone();

		if ( !panel.IsValid() )
			return;

		WeaponInfoPanel = panel.Components.Get<WeaponInfo>();
		if ( !WeaponInfoPanel.IsValid() )
			return;

		WeaponInfoPanel.Target = Equipment?.Grub?.GameObject;
		WeaponInfoPanel.Weapon = this;
	}

	public virtual void OnHolster()
	{
		IsFiring = false;
		WeaponInfoPanel?.GameObject?.Destroy();
		if ( ChargeGauge.IsValid() && ChargeGauge.GameObject.IsValid() )
			ChargeGauge.GameObject.Enabled = false;
	}

	protected virtual void FireFinished()
	{
		IsFiring = false;
		TimesUsed++;

		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.AutomaticRefocus = true;

		if ( TimesUsed >= MaxUses )
		{
			if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
				return;

			var grub = Equipment.Grub;

			// re-write this nonsense next time you see this and have free time

			if ( !CanSwapAfterUse )
			{
				if ( grub.Owner.IsValid() )
					grub.Owner.HasFiredThisTurn = true;
				// Gamemode.FFA.UseTurn( true );
			}
			else
			{
				if ( grub.Owner.Inventory.IsValid() )
					grub.Owner.Inventory.Holster( grub.Owner.Inventory.ActiveSlot );
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

		if ( !ChargeGauge.IsValid() )
		{
			ChargeGauge = new GameObject().Components.Create<SkinnedModelRenderer>();
			ChargeGauge.Model = Model.Load( "particles/weaponcharge/weapon_charge.vmdl" );
		}
		ChargeGauge.GameObject.Enabled = true;

		IsCharging = true;

		var muzzle = GetMuzzlePosition();

		ChargeGauge.WorldPosition = muzzle.Position;
		ChargeGauge.WorldRotation = Rotation.LookAt( GetMuzzleForward() ) * Rotation.FromPitch( 90 );
		ChargeGauge.SceneModel.Attributes.Set( "charge", _weaponCharge / 100f );

		_weaponCharge = (int)Math.Clamp( _startedCharging / 2f * 100f, 0f, 100f );

		if ( Input.UsingController ) Input.TriggerHaptics( 0, _weaponCharge / 100f, rightTrigger: _weaponCharge / 100f );
	}

	public Vector3 GetStartPosition( bool isDroppable = false )
	{
		if ( !Scene.IsValid() || !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return Vector3.Zero;

		if ( FiringType is FiringType.Cursor )
			return Vector3.Zero;

		var grub = Equipment.Grub;
		var controller = grub.CharacterController;

		if ( !controller.IsValid() )
			return Vector3.Zero;

		if ( isDroppable )
		{
			// Perform a forward trace to find the position to drop the item
			var dropTr = Scene.Trace.Ray( grub.EyePosition.Position,
					grub.EyePosition.Position + grub.WorldRotation.Forward * 25f )
				.IgnoreGameObjectHierarchy( grub.GameObject )
				.IgnoreGameObject( GameObject )
				.Radius( 1f )
				.Run();

			var startPosition = dropTr.EndPosition;

			if ( dropTr.Hit )
			{
				startPosition -= grub.WorldRotation.Forward * 10f;
			}

			return startPosition.WithY( 512 );
		}

		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return grub.EyePosition.Position + grub.WorldRotation.Forward * 4f;

		var tr = Scene.Trace.Ray( controller.BoundingBox.Center + grub.WorldPosition, muzzle.Value.Position )
			.IgnoreGameObjectHierarchy( grub.GameObject )
			.WithoutTags( "projectile" )
			.Radius( 1f )
			.Run();

		return tr.EndPosition.WithY( 512 );
	}

	public Transform GetMuzzlePosition()
	{
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return Transform.World;

		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		return muzzle ?? Equipment.Grub?.EyePosition ?? Transform.World;
	}

	public Vector3 GetMuzzleForward()
	{
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() || !Equipment.Grub.PlayerController.IsValid() )
			return 0f;

		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		if ( !muzzle.HasValue )
			return Equipment.Grub.PlayerController.EyeRotation.Forward * Equipment.Grub.PlayerController.Facing;
		return muzzle.Value.Rotation.Forward;
	}

	public string GetFireInputActionDescription()
	{
		return FiringType switch
		{
			FiringType.Instant => "Fire",
			FiringType.Complex => "Fire",
			FiringType.Charged => "Fire (Hold)",
			FiringType.Continuous => "Fire (Hold)",
			FiringType.Cursor => "Set Target",
			_ => string.Empty
		};
	}

	[Broadcast]
	private void FireEffects()
	{
		Sound.Play( UseSound, GetStartPosition() );
	}
}
