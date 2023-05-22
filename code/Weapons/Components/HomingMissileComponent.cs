namespace Grubs;

[Prefab]
public partial class HomingMissileComponent : GadgetWeaponComponent
{
	[Net]
	public ModelEntity TargetPreview { get; set; }
	private bool _isTargetSet;

	public override void OnDeploy()
	{
		_isTargetSet = false;
		Weapon.FiringType = FiringType.Cursor;
		Weapon.ShowReticle = false;

		if ( !Game.IsServer )
			return;

		TargetPreview = new ModelEntity( "models/weapons/targetindicator/targetindicator.vmdl" );
		TargetPreview.SetupPhysicsFromModel( PhysicsMotionType.Static );
		TargetPreview.Tags.Add( Tag.Preview );
		TargetPreview.Owner = Grub;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Game.IsServer )
		{
			if ( TargetPreview.IsValid() )
				TargetPreview.Delete();
		}
		else
		{
			Grub.Player.GrubsCamera.AutomaticRefocus = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !TargetPreview.IsValid() )
			return;

		TargetPreview.EnableDrawing = _isTargetSet || Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;

		if ( !_isTargetSet )
		{
			TargetPreview.Position = Grub.Player.MousePosition.WithY( -33 );
			TargetPreview.Rotation = Rotation.Lerp( TargetPreview.Rotation, TargetPreview.Rotation.RotateAroundAxis( Vector3.Right, 200 ), Time.Delta );
		}

		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;
		UI.Cursor.Enabled( "Weapon", Weapon.FiringType == FiringType.Cursor );
	}

	// Fire cursor shrimply locks the target preview from moving and changes the fire type
	// to be charged. Since the firing type is charged, base.Simulate(...) will handle charging
	// and firing the missile.
	public override void FireCursor()
	{
		_isTargetSet = true;
		IsFiring = false;
		Weapon.FiringType = FiringType.Charged;
		Weapon.ShowReticle = true;
		Weapon.PlayScreenSound( "ui_button_click" );
	}
}
