namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Weapon" ), Category( "Equipment" )]
public partial class WeaponComponent : Component
{
	public delegate void OnFireDelegate( int charge );

	[Property] public required EquipmentComponent Equipment { get; set; }

	[Property] public FiringType FiringType { get; set; } = FiringType.Instant;
	[Property] public OnFireDelegate? OnFire { get; set; }

	private int _weaponCharge;

	protected override void OnUpdate()
	{
		if ( IsProxy || !Equipment.Deployed )
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
				OnFire?.Invoke( _weaponCharge );
				_weaponCharge = 0;
			}
		}

		if ( Input.Pressed( "fire" ) )
		{
			OnFire?.Invoke( 100 );
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
