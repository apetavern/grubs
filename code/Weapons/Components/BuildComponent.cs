namespace Grubs;

[Prefab]
public partial class BuildComponent : WeaponComponent
{

	[Prefab]
	public string TextureToStamp { get; set; }

	[Net]
	public Vector2 MousePosition { get; set; }

	public ModelEntity GirderPreview { get; set; }

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );
		if ( Game.IsClient && !IsFiring && !Weapon.HasFired )
		{
			if ( GirderPreview != null && GirderPreview.IsValid )
			{
				GirderPreview.Position = Grub.Player.MousePosition;
				if ( Trace.Body( GirderPreview.PhysicsBody, Grub.Player.MousePosition ).Run().StartedSolid )
				{
					GirderPreview.RenderColor = Color.Red;
				}
				else
				{
					GirderPreview.RenderColor = Color.Green;
				}
			}
			else
			{
				GirderPreview = new ModelEntity( "models/tools/girders/girderpreview.vmdl" );
				GirderPreview.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
				GirderPreview.Owner = Grub;
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
		if ( GirderPreview != null && GirderPreview.IsValid )
		{
			GirderPreview.Delete();
		}
		GamemodeSystem.Instance.GameWorld.AddTextureStamp( TextureToStamp, Grub.Player.MousePosition );

		FireFinished();
	}

	public override void FireFinished()
	{
		base.FireFinished();
		if ( GirderPreview != null && GirderPreview.IsValid )
		{
			GirderPreview.Delete();
		}
		if ( Game.IsClient )
			Event.Run( "pointer.disabled" );
	}
}
