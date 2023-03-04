namespace Grubs;

[Prefab]
public partial class BuildComponent : WeaponComponent
{

	[Prefab]
	public string TextureToStamp { get; set; }

	[Net]
	public Vector2 MousePosition { get; set; }

	public ModelEntity GirderPreview { get; set; }

	[Net, Predicted]
	public float RotationAngle { get; set; }

	public override void OnDeploy()
	{
		if ( !Game.IsClient )
			return;

		GirderPreview = new ModelEntity( "models/tools/girders/girderpreview.vmdl" );
		GirderPreview.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		GirderPreview.Owner = Grub;
	}

	public override void OnHolster()
	{
		DeleteGirderPreview();
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		RotationAngle += Input.MouseWheel * 10f;

		if ( Game.IsClient && !IsFiring && !Weapon.HasFired )
		{
			if ( GirderPreview.IsValid() )
			{
				GirderPreview.Position = Grub.Player.MousePosition;
				GirderPreview.Rotation = Rotation.Identity * new Angles( RotationAngle, 0, 0 ).ToRotation();

				Grub.Player.GrubsCamera.Distance = 512f;
				Grub.Player.GrubsCamera.DistanceScrollRate = 0f;

				if ( Trace.Body( GirderPreview.PhysicsBody, Grub.Player.MousePosition ).Run().StartedSolid )
				{
					GirderPreview.RenderColor = Color.Red;
				}
				else
				{
					GirderPreview.RenderColor = Color.Green;
				}
			}
		}

		if ( IsFiring )
		{
			Fire();
		}
	}

	public override void FireCursor()
	{
		IsFiring = false;
		GamemodeSystem.Instance.GameWorld.AddTextureStamp( TextureToStamp, Grub.Player.MousePosition, RotationAngle );
		FireFinished();
	}

	public override void FireFinished()
	{
		DeleteGirderPreview();
		base.FireFinished();
	}

	private void DeleteGirderPreview()
	{
		if ( GirderPreview.IsValid() )
		{
			GirderPreview.Delete();
			Grub.Player.GrubsCamera.DistanceScrollRate = 32f;
		}

		if ( Game.IsClient )
			Event.Run( "pointer.disabled" );
	}
}
