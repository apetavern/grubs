using Grubs.Gamemodes;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Weapon" ), Category( "Equipment" )]
public partial class WeaponComponent : Component
{
	public delegate void OnFireDelegate( int charge );

	[Property] public required EquipmentComponent Equipment { get; set; }

	[Property] public float Cooldown { get; set; } = 2f;
	[Property] public bool CanFireWhileMoving { get; set; } = false;
	[Property] public int MaxUses { get; set; } = 1;
	[Property] public FiringType FiringType { get; set; } = FiringType.Instant;
	[Property] public OnFireDelegate OnFire { get; set; }

	public bool IsFiring { get; set; }
	public TimeSince TimeSinceLastUsed { get; set; }
	public int TimesUsed { get; set; }

	private int _weaponCharge;

	protected override void OnStart()
	{
		base.OnStart();

		TimeSinceLastUsed = Cooldown;
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

				if ( OnFire is not null )
					OnFire.Invoke( _weaponCharge );
				else
					FireCharged( _weaponCharge );

				TimeSinceLastUsed = 0;
				_weaponCharge = 0;

				FireFinished();
			}
		}

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

	protected virtual void FireImmediate() { }
	protected virtual void FireCharged( int charge ) { }

	protected virtual void FireFinished()
	{
		IsFiring = false;
		TimesUsed++;

		if ( TimesUsed >= MaxUses )
		{
			Gamemode.FFA.UseTurn( true );
			if ( Equipment.Grub is { } grub )
				grub.Player.HasFiredThisTurn = true;
		}
	}

	protected void OnChargedHeld()
	{
		_weaponCharge++;
		_weaponCharge.Clamp( 0, 100 );
	}

	public Vector3 GetStartPosition( bool isDroppable = false )
	{
		if ( FiringType is FiringType.Cursor )
			return Vector3.Zero;

		if ( isDroppable )
			return Transform.Position.WithY( 0f );

		if ( Equipment.Grub is not { } grub )
			return Vector3.Zero;

		var muzzle = Equipment.Model.GetAttachment( "muzzle" );
		if ( muzzle is null )
			return grub.Transform.Position;

		var controller = grub.CharacterController;
		var tr = Scene.Trace.Ray( controller.BoundingBox.Center + grub.Transform.Position, muzzle.Value.Position )
			.IgnoreGameObjectHierarchy( grub.GameObject )
			.WithoutTags( "projectile" )
			.Radius( 1f )
			.Run();

		return tr.EndPosition.WithY( 512f );
	}
}
