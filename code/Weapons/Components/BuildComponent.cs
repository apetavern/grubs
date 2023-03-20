namespace Grubs;

[Prefab]
public partial class BuildComponent : WeaponComponent
{
	[Prefab, ResourceType( "sound" )]
	public string BuildSound { get; set; }

	[Prefab]
	public string TextureToStamp { get; set; }

	[Net]
	public Vector2 MousePosition { get; set; }

	[Net]
	public ModelEntity GirderPreview { get; set; }

	[Net, Predicted]
	public float RotationAngle { get; set; }

	public override void OnDeploy()
	{
		if ( !Game.IsServer )
			return;

		GirderPreview = new ModelEntity( "models/tools/girders/girderpreview.vmdl" );
		GirderPreview.SetupPhysicsFromModel( PhysicsMotionType.Static );
		GirderPreview.Owner = Grub;
		GirderPreview.Tags.Add( "trigger" );
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Game.IsServer && GirderPreview.IsValid() )
			GirderPreview.Delete();

		Grub.Player.GrubsCamera.DistanceScrollRate = 32f;

		Event.Run( GrubsEvent.Player.PointerEventChanged, false );
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !GirderPreview.IsValid() )
			return;

		RotationAngle += Input.MouseWheel * 10f;

		GirderPreview.Position = Grub.Player.MousePosition;
		GirderPreview.Rotation = Rotation.Identity * new Angles( RotationAngle, 0, 0 ).ToRotation();

		var isValidPlacement = !Trace.Body( GirderPreview.PhysicsBody, Grub.Player.MousePosition ).Ignore( GirderPreview ).Run().Hit;
		GirderPreview.RenderColor = isValidPlacement ? Color.Green : Color.Red;

		Grub.Player.GrubsCamera.Distance = 512f;
		Grub.Player.GrubsCamera.DistanceScrollRate = 0f;

		if ( IsFiring )
		{
			if ( isValidPlacement )
				Fire();

			IsFiring = false;
		}
	}

	public override void FireCursor()
	{
		Weapon.PlayScreenSound( To.Everyone, BuildSound );

		GamemodeSystem.Instance.GameWorld.AddTextureStamp( TextureToStamp, Grub.Player.MousePosition, RotationAngle );
		FireFinished();
	}
}
