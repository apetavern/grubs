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

		// Change the direction of the airstrike based on which way our Grub is facing.
		RightToLeft = Grub.Facing < 0 ? true : false;

		Grub.Player.GrubsCamera.CanScroll = !Weapon.HasChargesRemaining;
		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		if ( Weapon.HasChargesRemaining )
			Grub.Player.GrubsCamera.Distance = 1024f;

		AirstrikeCursor.Position = Grub.Player.MousePosition;
		AirstrikeCursor.Rotation = RightToLeft ? Rotation.Identity : Rotation.Identity * new Angles( -90, 0, 0 ).ToRotation();

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
		if ( Game.IsServer && PrefabLibrary.TrySpawn<Gadget>( PlanePrefab.ResourcePath, out var plane ) )
		{

			plane.Owner = Weapon.Owner;
			plane.Position = new Vector3( AirstrikePosition.x + (RightToLeft ? AirstrikeGadgetComponent.SpawnOffsetX : -AirstrikeGadgetComponent.SpawnOffsetX),
			AirstrikeGadgetComponent.SpawnOffsetY,
			GrubsConfig.TerrainHeight + AirstrikeGadgetComponent.SpawnOffsetZ );
			plane.Rotation = RightToLeft ? Rotation.Identity * new Angles( 180, 0, 180 ).ToRotation() : Rotation.Identity;
			Grub.Player.Gadgets.Add( plane );

			var airstrike = plane.Components.Get<AirstrikeGadgetComponent>();
			airstrike.TargetPosition = AirstrikePosition;
			airstrike.BombingDirection = RightToLeft ? Vector3.Backward : Vector3.Forward;
		}

		FireFinished();
	}

	public override void FireFinished()
	{
		base.FireFinished();
		Weapon.Holster( Grub );
	}
}