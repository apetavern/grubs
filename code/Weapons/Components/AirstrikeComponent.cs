namespace Grubs;

[Prefab]
public partial class AirstrikeComponent : WeaponComponent
{
	[Prefab]
	public Prefab Payload { get; set; }

	[Prefab]
	public int PayloadCount { get; set; }

	[Net]
	private ModelEntity AirstrikeCursor { get; set; }

	/// <summary>
	/// -1 for "right to left"
	/// 1 for "left to right"
	/// </summary>
	/// <value></value>
	[Net, Predicted]
	private int Direction { get; set; }

	[Net]
	public Vector3 AirstrikeTarget { get; private set; }

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
			Direction = Direction < 0 ? 1 : -1;


		Grub.Player.GrubsCamera.CanScroll = !Weapon.HasChargesRemaining;
		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		if ( Weapon.HasChargesRemaining )
			Grub.Player.GrubsCamera.Distance = 1024f;

		AirstrikeCursor.Position = Grub.Player.MousePosition;
		AirstrikeCursor.Rotation = Direction > 0 ? Rotation.Identity : Rotation.Identity * new Angles( 180, 0, 180 ).ToRotation();

		if ( IsFiring && AirstrikeCursor.EnableDrawing )
		{
			AirstrikeTarget = Grub.Player.MousePosition.WithY( 0 ).WithZ( 0 );
			Fire();
		}
		else
			IsFiring = false;
	}

	public override void FireCursor()
	{
		if ( Game.IsServer && PrefabLibrary.TrySpawn<AirstrikePlane>( "prefabs/weapons/airstrike/airstrike_plane.prefab", out var plane ) )
		{
			plane.Position = AirstrikeTarget.WithZ( 512 );
			plane.Rotation = AirstrikeCursor.Rotation;
			plane.DeleteAsync( 5 );
		}

		FireFinished();
	}
}
