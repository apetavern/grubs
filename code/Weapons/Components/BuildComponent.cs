using Sandbox.Sdf;

namespace Grubs;

[Prefab]
public partial class BuildComponent : WeaponComponent
{
	[Prefab, ResourceType( "sound" )]
	public string BuildSound { get; set; }

	[Prefab]
	public string TextureToStamp { get; set; }

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
		GirderPreview.Tags.Add( "preview" );
		GirderPreview.Owner = Grub;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Game.IsServer )
		{
			if ( GirderPreview.IsValid() )
				GirderPreview.Delete();
		}
		else
		{
			Grub.Player.GrubsCamera.CanScroll = true;
			Grub.Player.GrubsCamera.AutomaticCentering = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !GirderPreview.IsValid() )
			return;

		GirderPreview.EnableDrawing = Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;

		RotationAngle += Input.MouseWheel * 10f;

		if ( RotationAngle < -90 )
			RotationAngle += 180;

		if ( RotationAngle > 90 )
			RotationAngle -= 180;

		Grub.Player.GrubsCamera.CanScroll = !Weapon.HasChargesRemaining;
		Grub.Player.GrubsCamera.AutomaticCentering = !Weapon.HasChargesRemaining;

		if ( Weapon.HasChargesRemaining )
			Grub.Player.GrubsCamera.Distance = 1024f;

		GirderPreview.Position = Grub.Player.MousePosition;
		GirderPreview.Rotation = Rotation.Identity * new Angles( RotationAngle, 0, 0 ).ToRotation();

		var isValidPlacement = !Trace.Body( GirderPreview.PhysicsBody, Grub.Player.MousePosition ).Ignore( GirderPreview ).Run().Hit;
		GirderPreview.RenderColor = isValidPlacement ? Color.Green : Color.Red;

		if ( IsFiring && GirderPreview.EnableDrawing && isValidPlacement )
			Fire();
		else
			IsFiring = false;
	}

	public override void FireCursor()
	{
		Weapon.PlayScreenSound( To.Everyone, BuildSound );

		if ( Game.IsServer )
		{
			var girderTexture = Texture.Load( FileSystem.Mounted, "textures/texturestamps/girder_sdf.png" );

			var terrain = GamemodeSystem.Instance.Terrain;
			var materials = new Dictionary<Sdf2DMaterial, float>();
			foreach ( var mat in terrain.GetGirderMaterials() )
				materials.Add( mat, 0f );

			terrain.AddTexture(
				girderTexture,
				4,
				girderTexture.Width * 2f,
				new Vector2( Grub.Player.MousePosition.x - 5f, Grub.Player.MousePosition.z ),
				new Rotation2D( RotationAngle ),
				materials );
		}

		FireFinished();
	}
}
