using Grubs.Utils;

namespace Grubs;

[Prefab]
public partial class AirstrikeGadgetComponent : GadgetComponent
{
	[Prefab]
	public Prefab Projectile { get; set; }

	[Prefab]
	public float DropRateSeconds { get; set; } = 0.2f;

	[Prefab]
	public int ProjectileCount { get; set; } = 1;

	[Prefab, Net, ResourceType( "vpcf" )]
	public string ConTrail { get; set; }

	/// <summary>
	/// Usually only Vector3.Backward or Vector3.Forward.
	/// Vector3.Forward for Left -> Right and vice-versa.
	/// </summary>
	[Net]
	public Vector3 BombingDirection { get; set; }

	// Network this so the client can play the bombing animation at the right time.
	[Net]
	public Vector3 TargetPosition { get; set; }

	public bool HasReachedTarget { get; private set; }

	public const float SpawnOffsetX = 3000;
	public const float SpawnOffsetY = -30;
	public const float SpawnOffsetZ = 64;

	private Particles _trailLeft;
	private Particles _trailRight;
	private Sound _engineSound;
	private const float _dropPayloadDistanceThreshold = 950;
	private const float _planeFlySpeed = 23;

	public override void ClientSpawn()
	{
		_engineSound = Gadget.PlaySound( "sounds/airstrike/plane_engine_loop.sound" );
		_trailLeft = Particles.Create( ConTrail, Gadget, "trailLeft" );
		_trailRight = Particles.Create( ConTrail, Gadget, "trailRight" );
	}

	public override void Simulate( IClient client )
	{
		Gadget.Position += BombingDirection * _planeFlySpeed;
		Gadget.Position += new Vector3( 0, 0, MathF.Sin( Time.Tick / 10f ) * 2.5f );
		Gadget.Rotation *= Rotation.Identity * new Angles( 0, 0, MathF.Sin( Time.Tick / 10f ) * 0.5f ).ToRotation();

		var distanceToTarget = Gadget.Position.WithY( 0 ).WithZ( 0 ).Distance( TargetPosition.WithY( 0 ).WithZ( 0 ) );
		var planeIsOutOfBounds = Gadget.Position.WithY( 0 ).WithZ( 0 ).Distance( GrubsGame.Instance.Terrain.Position.WithY( 0 ).WithZ( 0 ) ) >= GrubsConfig.TerrainLength * 3;

		if ( distanceToTarget <= _dropPayloadDistanceThreshold + 320 )
			Animate();

		if ( distanceToTarget <= _dropPayloadDistanceThreshold && !HasReachedTarget )
		{
			HasReachedTarget = true;

			if ( Game.IsServer )
			{
				Gadget.ShouldCameraFollow = false;
				DropPayload();
			}

			Gadget.PlaySound( "sounds/airstrike/plane_bay_door_open.sound" );
		}

		if ( HasReachedTarget && planeIsOutOfBounds )
		{
			if ( Game.IsClient )
			{
				_trailLeft.Destroy();
				_trailRight.Destroy();
				_engineSound.Stop();
			}

			if ( Game.IsServer )
				Gadget.Delete();
		}
	}

	private async void DropPayload()
	{
		var dropAttachment = Gadget.GetAttachment( "droppoint", false );
		var angularDirection = BombingDirection * _planeFlySpeed;
		for ( int i = 0; i < ProjectileCount; i++ )
		{
			await GameTask.DelaySeconds( DropRateSeconds );

			if ( !PrefabLibrary.TrySpawn<Gadget>( Projectile.ResourcePath, out var bomb ) )
				continue;

			Gadget.Grub.AssignGadget( bomb );
			bomb.Position = dropAttachment.HasValue ? Gadget.Position + dropAttachment.Value.Position : Gadget.Position;

			if ( i == 0 )
				bomb.ShouldCameraFollow = true;

			if ( bomb.Components.TryGet<ArcPhysicsGadgetComponent>( out var arcPhysics ) )
				arcPhysics.Start( bomb.Position, Vector3.Down + angularDirection, 1 );
		}
	}

	private async void Animate()
	{
		Gadget.SetAnimParameter( "open", true );
		await GameTask.DelaySeconds( 1 );
		Gadget.SetAnimParameter( "open", false );
	}
}
