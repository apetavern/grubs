namespace Grubs;

[Prefab]
public partial class TeleportComponent : WeaponComponent
{
	[Net]
	public AnimatedEntity TeleportPreview { get; set; }

	public override void OnDeploy()
	{
		if ( !Game.IsServer )
			return;

		TeleportPreview = new AnimatedEntity( "models/citizenworm.vmdl" );
		TeleportPreview.SetupPhysicsFromModel( PhysicsMotionType.Static );
		TeleportPreview.SetAnimParameter( "grounded", true );
		TeleportPreview.Tags.Add( "trigger" );
		TeleportPreview.Owner = Grub;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Game.IsServer && TeleportPreview.IsValid() )
			TeleportPreview.Delete();
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !TeleportPreview.IsValid() )
			return;

		TeleportPreview.EnableDrawing = Grub.Controller.ShouldShowWeapon();

		if ( !TeleportPreview.EnableDrawing )
			return;

		TeleportPreview.Position = Grub.Player.MousePosition;

		var isValidPlacement = !Trace.Box( Grub.Controller.Hull, Grub.Player.MousePosition, Grub.Player.MousePosition ).Ignore( TeleportPreview ).Run().Hit;
		TeleportPreview.RenderColor = isValidPlacement ? Color.Green : Color.Red;

		if ( !IsFiring )
			return;

		if ( isValidPlacement )
			Fire();

		IsFiring = false;
	}

	public override void FireCursor()
	{
		IsFiring = false;
		Grub.Position = Grub.Player.MousePosition;

		FireFinished();
	}
}
