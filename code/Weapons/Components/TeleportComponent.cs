namespace Grubs;

[Prefab]
public partial class TeleportComponent : WeaponComponent
{
	[Prefab, ResourceType( "sound" )]
	public string UseSound { get; set; }

	[Net]
	public AnimatedEntity TeleportPreview { get; set; }

	public override void OnDeploy()
	{
		if ( !Game.IsServer )
			return;

		TeleportPreview = new AnimatedEntity( "models/citizenworm.vmdl" );
		TeleportPreview.SetupPhysicsFromModel( PhysicsMotionType.Static );
		TeleportPreview.SetMaterialGroup( "Teleport_Preview" );
		TeleportPreview.SetAnimParameter( "grounded", true );
		TeleportPreview.Tags.Add( "preview" );
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

		TeleportPreview.EnableDrawing = Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;
		TeleportPreview.Position = Grub.Player.MousePosition;
		TeleportPreview.Rotation = Grub.Rotation;

		var isValidPlacement = !Trace.Box( Grub.Controller.Hull, Grub.Player.MousePosition, Grub.Player.MousePosition ).Ignore( TeleportPreview ).Run().Hit;
		TeleportPreview.RenderColor = (isValidPlacement ? Color.Green : Color.Red).WithAlpha( 0.5f );

		if ( IsFiring && TeleportPreview.EnableDrawing && isValidPlacement )
			Fire();
		else
			IsFiring = false;
	}

	public override void FireCursor()
	{
		Weapon.PlayScreenSound( UseSound );

		Particles.Create( "particles/teleport/teleport_up.vpcf", Grub.EyePosition );
		Grub.Position = Grub.Player.MousePosition;
		Particles.Create( "particles/teleport/teleport_down.vpcf", Grub.EyePosition );

		FireFinished();
	}
}
