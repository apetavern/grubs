namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Weapon" )]
[Category( "Equipment" )]
public partial class WeaponComponent : Component
{
	public delegate void OnFireDelegate( int charge );

	[Property] public required EquipmentComponent Equipment { get; set; }

	[Property] public FiringType FiringType { get; set; } = FiringType.Instant;
	[Property] public OnFireDelegate? OnFire { get; set; }

	private int _weaponCharge = 0;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "fire" ) )
		{
			OnFire?.Invoke( 100 );
		}
	}
}
