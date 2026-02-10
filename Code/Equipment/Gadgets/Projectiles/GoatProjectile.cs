using Sandbox;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Goat Projectile" ), Category( "Equipment" )]
public class GoatProjectile : Projectile
{
	[Property] public bool Droppable { get; set; } = false;
	[Property] public Rigidbody PhysicsBody { get; set; }
	[Property] public bool SetPositionOnStart { get; set; } = true;

	[Property] private SoundEvent CollisionSound { get; set; }
	private Vector3 TargetLookAt { get; set; }

	private Rotation StartRotation { get; set; }

	public override bool Resolved => PhysicsBody?.Velocity.IsNearlyZero( 0.1f ) ?? true;

	protected override void OnStart()
	{
		if ( !Source.IsValid() )
			return;

		WorldPosition = Source.GetStartPosition( Droppable );

		var dir = Source.GetMuzzleForward() + Vector3.Up;
		WorldPosition += dir * 16f;
		WorldRotation = Rotation.LookAt( dir.WithZ( 0f ) );
		StartRotation = WorldRotation; 

		if ( Droppable )
			return;

		if ( !PlayerController.IsValid() )
			return;

		if ( !PhysicsBody.IsValid() )
			return;
		PhysicsBody.ApplyImpulse( dir * Charge * ProjectileSpeed );

		Model.Set( "active", true );
	}

	TimeSince timeSinceLastBleat;

	protected override void OnUpdate()
	{
		if ( !PlayerController.IsValid() || !PhysicsBody.IsValid() )
			return;

		base.OnUpdate();

		Model.Set( "active", true );
		TargetLookAt = StartRotation.Forward + Vector3.Up * PhysicsBody.Velocity.z / 750f;
		WorldRotation = Rotation.Lerp( WorldRotation, Rotation.LookAt( TargetLookAt, Vector3.Up ), Time.Delta * 10f );

		var groundTrace = Scene.Trace.Body( PhysicsBody, WorldPosition - Vector3.Up * 5f )
			.IgnoreGameObject(GameObject )
			.Run();

		var ceilingTrace = Scene.Trace.Body( PhysicsBody, WorldPosition + Vector3.Up * 5f )
		.IgnoreGameObject( GameObject )
		.Run();

		Model.Set( "grounded", groundTrace.Hit && !ceilingTrace.Hit );

		if(groundTrace.Hit && !ceilingTrace.Hit)
		{
			if ( CollisionSound is not null && timeSinceLastBleat > 0.2f )
			{
				Sound.Play( CollisionSound, WorldPosition );
				timeSinceLastBleat = 0f;
			}

			PhysicsBody.Velocity = (Vector3.Up + WorldRotation.Forward) * ProjectileSpeed;
		}
	}
}
