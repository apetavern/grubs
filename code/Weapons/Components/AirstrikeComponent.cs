namespace Grubs;

[Prefab]
public partial class AirstrikeComponent : WeaponComponent
{
	[Prefab]
	public Prefab PlanePrefab { get; set; }

	[Net, Predicted]
	private bool RightToLeft { get; set; }

	[Net]
	private ModelEntity AirstrikeCursor { get; set; }

	[Net]
	public Vector3 AirstrikePosition { get; private set; }

	public override void OnDeploy()
	{
		if ( !Game.IsServer )
			return;

		AirstrikeCursor = new ModelEntity( "models/weapons/airstrikes/plane.vmdl" );
		AirstrikeCursor.Scale = 0.25f;
		AirstrikeCursor.SetupPhysicsFromModel( PhysicsMotionType.Static );
		AirstrikeCursor.Tags.Add( Tag.Preview );
		AirstrikeCursor.Owner = Grub;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Game.IsServer )
		{
			if ( AirstrikeCursor.IsValid() )
				AirstrikeCursor.Delete();
		}
		else
		{
			Grub.Player.GrubsCamera.CanScroll = true;
			Grub.Player.GrubsCamera.AutomaticRefocus = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !AirstrikeCursor.IsValid() )
			return;

		AirstrikeCursor.EnableDrawing = Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;

		// TODO: Use a dedicated InputAction binding.
		if ( Input.Released( InputAction.CameraPan ) )
			RightToLeft = !RightToLeft;


		Grub.Player.GrubsCamera.CanScroll = !Weapon.HasChargesRemaining;
		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		if ( Weapon.HasChargesRemaining )
			Grub.Player.GrubsCamera.Distance = 1024f;

		AirstrikeCursor.Position = Grub.Player.MousePosition;
		AirstrikeCursor.Rotation = RightToLeft ? Rotation.Identity * new Angles( 180, 0, 180 ).ToRotation() : Rotation.Identity;

		if ( IsFiring && AirstrikeCursor.EnableDrawing )
		{
			AirstrikePosition = Grub.Player.MousePosition;
			Fire();
		}
		else
			IsFiring = false;
	}

	public override void FireCursor()
	{
		if ( Game.IsServer && PrefabLibrary.TrySpawn<AirstrikePlane>( PlanePrefab.ResourcePath, out var plane ) )
		{
			const float zOffset = 64;
			const float xOffset = 128;

			var rootPosition = GrubsGame.Instance.Terrain.Position.WithY( 0 ).WithZ( GrubsConfig.TerrainHeight + zOffset );
			var direction = RightToLeft ? Vector3.Forward : Vector3.Backward;
			var planeSpawnPosition = rootPosition + direction * GrubsConfig.TerrainLength + xOffset;

			GamemodeSystem.Instance.CameraTarget = plane;
			plane.Owner = Weapon.Owner;
			plane.TargetPosition = AirstrikePosition;
			plane.RightToLeft = RightToLeft;
			plane.Position = planeSpawnPosition;
			plane.Rotation = AirstrikeCursor.Rotation;
		}

		FireFinished();
	}
}
