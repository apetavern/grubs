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

		if ( Game.IsServer )
		{
			if ( TeleportPreview.IsValid() )
				TeleportPreview.Delete();
		}
		else
		{
			Grub.Player.GrubsCamera.AutomaticRefocus = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !TeleportPreview.IsValid() )
			return;

		TeleportPreview.EnableDrawing = Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;
		TeleportPreview.Position = Grub.Player.MousePosition;
		TeleportPreview.Rotation = Grub.Rotation;

		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		// Causes a little bit of delay on the teleport preview, but clientside traces
		// here are causing some odd behaviour.
		if ( Game.IsServer )
		{
			var isValidPlacement = CheckValidPlacement( Grub.Player.MousePosition );
			TeleportPreview.RenderColor = (isValidPlacement ? Color.Green : Color.Red).WithAlpha( 0.5f );

			if ( IsFiring && TeleportPreview.EnableDrawing && isValidPlacement )
				Fire();
			else
				IsFiring = false;
		}
	}

	public override void FireCursor()
	{
		Weapon.PlayScreenSound( UseSound );

		Particles.Create( "particles/teleport/teleport_up.vpcf", Grub.EyePosition );
		Grub.Position = Grub.Player.MousePosition;
		Particles.Create( "particles/teleport/teleport_down.vpcf", Grub.EyePosition );

		FireFinished();
	}

	private bool CheckValidPlacement( Vector3 mousePosition )
	{
		var trLocation = Trace.Box( Grub.Controller.Hull, mousePosition, mousePosition )
			.Ignore( TeleportPreview )
			.Run();

		var trTerrain = Trace.Ray( trLocation.EndPosition, trLocation.EndPosition + Vector3.Right * 64f )
			.WithAnyTags( "solid" )
			.Size( 1f )
			.Ignore( TeleportPreview )
			.Run();

		var terrain = GrubsGame.Instance.Terrain;
		var exceedsTerrainHeight = false;
		if ( GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture && trLocation.EndPosition.z >= terrain.WorldTextureHeight - 64f )
			exceedsTerrainHeight = true;

		return !trLocation.Hit && !trTerrain.Hit && !exceedsTerrainHeight;
	}
}
