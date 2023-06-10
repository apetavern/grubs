namespace Grubs;

[Prefab]
public partial class AirstrikeRemoteComponent : WeaponComponent
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

		AirstrikeCursor = new ModelEntity( "models/weapons/targetindicator/arrowindicator.vmdl" );
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
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !AirstrikeCursor.IsValid() )
			return;

		AirstrikeCursor.EnableDrawing = Grub.Controller.ShouldShowWeapon() && Weapon.HasChargesRemaining;

		// Change the direction of the airstrike based on which way our Grub is facing.
		RightToLeft = Grub.Facing < 0;

		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		AirstrikeCursor.Position = Grub.Player.MousePosition;
		AirstrikeCursor.Rotation = RightToLeft ? Rotation.Identity : Rotation.Identity * new Angles( -90, 0, 0 ).ToRotation();

		if ( IsFiring && AirstrikeCursor.EnableDrawing )
			Fire();
		else
			IsFiring = false;
	}

	public override void FireCursor()
	{
		if ( Game.IsServer && PrefabLibrary.TrySpawn<Gadget>( PlanePrefab.ResourcePath, out var plane ) )
		{
			AirstrikePosition = Grub.Player.MousePosition;

			Grub.AssignGadget( plane );

			plane.Position = new Vector3( AirstrikePosition.x + (RightToLeft ? GrubsConfig.TerrainLength * 1.5f : -GrubsConfig.TerrainLength * 1.5f),
			AirstrikeGadgetComponent.SpawnOffsetY,
			GrubsConfig.TerrainHeight + AirstrikeGadgetComponent.SpawnOffsetZ );
			plane.Rotation = RightToLeft ? Rotation.Identity * new Angles( 180, 0, 180 ).ToRotation() : Rotation.Identity;

			var airstrikeInfo = plane.Components.Get<AirstrikeGadgetComponent>();
			airstrikeInfo.TargetPosition = AirstrikePosition;
			airstrikeInfo.BombingDirection = RightToLeft ? Vector3.Backward : Vector3.Forward;
		}

		FireFinished();
	}
}
