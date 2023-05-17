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

		if ( !Game.IsServer )
			return;

		// TODO: This should be animated? Look into this before merge.
		TargetPreview = new ModelEntity( "models/weapons/targetindicator/targetindicator.vmdl" );
		TargetPreview.SetupPhysicsFromModel( PhysicsMotionType.Static );
		TargetPreview.Tags.Add( "preview" );
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

		TargetPreview.EnableDrawing = Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;

		if ( !_isTargetSet )
		{
			TargetPreview.Position = Grub.Player.MousePosition;
			TargetPreview.Rotation = Grub.Rotation;
		}

		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;
	}

	// Fire cursor shrimply locks the target preview from moving and changes the fire type
	// to be charged. Since the firing type is charged, base.Simulate(...) will handle charging
	// and firing the missile.
	public override void FireCursor()
	{
		_isTargetSet = true;
		IsFiring = false;
		Weapon.FiringType = FiringType.Charged;
	}
}
