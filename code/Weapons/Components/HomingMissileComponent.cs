namespace Grubs;

[Prefab]
public partial class HomingMissileComponent : GadgetWeaponComponent
{
	[Net, Predicted]
	public TargetPreview TargetPreview { get; set; }

	public override void OnDeploy()
	{
		TargetPreview = new();
		TargetPreview.Display( Grub );

		Weapon.FiringType = FiringType.Cursor;
		Weapon.ShowReticle = false;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		TargetPreview.Hide();

		if ( !Game.IsServer )
			Grub.Player.GrubsCamera.AutomaticRefocus = true;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		TargetPreview.Simulate( client );
	}

	// Shrimply locks the target preview from moving AND change the fire type
	// to be charged. Since the firing type is charged, base.Simulate(...) will handle charging
	// and firing the missile.
	public override void FireCursor()
	{
		TargetPreview.LockCursor();

		IsFiring = false;
		Weapon.FiringType = FiringType.Charged;
		Weapon.ShowReticle = true;
		Weapon.PlayScreenSound( "ui_button_click" );
	}
}
