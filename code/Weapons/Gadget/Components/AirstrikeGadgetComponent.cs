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

	[Net]
	public bool RightToLeft { get; set; }

	public Vector3 TargetPosition { get; set; }

	public bool HasReachedTarget { get; private set; }

	public const float SpawnOffsetX = 3000;
	public const float SpawnOffsetY = -30;
	public const float SpawnOffsetZ = 64;

	private const float _dropPayloadDistanceThreshold = 950;
	private float _planeSpeed = 23;
	private bool _logged = false;

	public override void Simulate( IClient client )
	{
		Gadget.Position += RightToLeft ? Vector3.Backward * _planeSpeed : Vector3.Forward * _planeSpeed;
		var distanceToTarget = Gadget.Position.WithY( 0 ).WithZ( 0 ).Distance( TargetPosition.WithY( 0 ).WithZ( 0 ) );

		if ( distanceToTarget <= _dropPayloadDistanceThreshold + 300 )
			Animate();

		if ( distanceToTarget <= _dropPayloadDistanceThreshold && !HasReachedTarget )
		{
			HasReachedTarget = true;

			if ( Game.IsServer )
			{
				Gadget.ShouldCameraFollow = false;
				DropPayload();
			}
		}

		if ( Game.IsServer && (HasReachedTarget && Gadget.Position.WithY( 0 ).WithZ( 0 ).Distance( GrubsGame.Instance.Terrain.Position.WithY( 0 ).WithZ( 0 ) ) >= GrubsConfig.TerrainLength * 3) )
			Gadget.Delete();
	}

	private async void DropPayload()
	{
		var dropAttachment = Gadget.GetAttachment( "droppoint", false );
		var direction = RightToLeft ? Vector3.Backward : Vector3.Forward;
		direction *= _planeSpeed;

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
				arcPhysics.Start( bomb.Position, Vector3.Down + direction, 1 );
		}
	}

	private async void Animate()
	{
		Gadget.SetAnimParameter( "open", true );
		await GameTask.DelaySeconds( 1 );
		Gadget.SetAnimParameter( "open", false );
	}
}
