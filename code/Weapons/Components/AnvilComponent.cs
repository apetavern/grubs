namespace Grubs;

[Prefab]
public partial class AnvilComponent : GadgetWeaponComponent
{
	[Net, Predicted]
	public TargetPreview TargetPreview { get; set; }

	public override void OnDeploy()
	{
		TargetPreview = new();
		TargetPreview.OnDeploy( Grub );

		Weapon.FiringType = FiringType.Cursor;
		Weapon.ShowReticle = false;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		TargetPreview.OnHolster();

		if ( !Game.IsServer )
			Grub.Player.GrubsCamera.AutomaticRefocus = true;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		TargetPreview.Simulate( client );
	}

	public override void FireCursor()
	{
		base.FireCursor();

		TargetPreview.FireCursor();
		Weapon.PlayScreenSound( "ui_button_click" );
	}
}
