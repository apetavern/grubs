namespace Grubs;

[Prefab]
public partial class AirstrikeComponent : WeaponComponent
{
	[Prefab]
	public Prefab PlanePrefab { get; set; }

	/// <summary>
	/// 1 for "left to right"
	/// -1 for "right to left"
	/// </summary>
	[Net, Predicted]
	private int Direction { get; set; }
	private bool LeftToRight => Direction > 0;

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
			Direction = LeftToRight ? -1 : 1;


		Grub.Player.GrubsCamera.CanScroll = !Weapon.HasChargesRemaining;
		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		if ( Weapon.HasChargesRemaining )
			Grub.Player.GrubsCamera.Distance = 1024f;

		AirstrikeCursor.Position = Grub.Player.MousePosition;
		AirstrikeCursor.Rotation = LeftToRight ? Rotation.Identity * new Angles( 180, 0, 180 ).ToRotation() : Rotation.Identity;

		if ( IsFiring && AirstrikeCursor.EnableDrawing )
		{
			AirstrikePosition = Grub.Player.MousePosition;
			DebugOverlay.Sphere( AirstrikePosition, 20, Color.Red, 20 );
			Fire();
		}
		else
			IsFiring = false;
	}

	public override void FireCursor()
	{
		if ( Game.IsServer && PrefabLibrary.TrySpawn<AirstrikePlane>( PlanePrefab.ResourcePath, out var plane ) )
		{
			var planeSpawnX = LeftToRight ? GrubsConfig.TerrainLength : -(GrubsConfig.TerrainLength / 2);
			plane.Owner = Weapon.Owner;
			plane.TargetX = AirstrikePosition.x;
			plane.LeftToRight = LeftToRight;
			plane.Position = new Vector3( planeSpawnX, 0, 512 );
			plane.Rotation = AirstrikeCursor.Rotation;
		}

		FireFinished();
	}
}
