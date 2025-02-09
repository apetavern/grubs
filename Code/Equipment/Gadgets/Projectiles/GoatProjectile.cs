namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Goat Projectile" ), Category( "Equipment" )]
public class GoatProjectile : Projectile, Component.ICollisionListener
{
	[Property] public bool Droppable { get; set; } = false;
	[Property] public Rigidbody PhysicsBody { get; set; }
	[Property] public bool SetPositionOnStart { get; set; } = true;

	[Property] private SoundEvent CollisionSound { get; set; }
	private Vector3 TargetLookAt { get; set; }

	public override bool Resolved => PhysicsBody?.Velocity.IsNearlyZero( 0.1f ) ?? true;

	protected override void OnStart()
	{
		if ( !Source.IsValid() )
			return;

		WorldPosition = Source.GetStartPosition( Droppable );

		if ( Droppable )
			return;

		if ( !PlayerController.IsValid() )
			return;

		var dir = (PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing) + Vector3.Up;
		WorldPosition += dir * 16f;

		if ( !PhysicsBody.IsValid() )
			return;
		PhysicsBody.ApplyImpulse( dir * Charge * ProjectileSpeed );

		Model.Set( "active", true );
	}

	protected override void OnUpdate()
	{
		if ( !PlayerController.IsValid() || !PhysicsBody.IsValid() )
			return;

		base.OnUpdate();
		Model.Set( "active", true );
		TargetLookAt = (PlayerController.EyeRotation.Forward.Normal.WithZ( 0 ) * PlayerController.Facing) + Vector3.Up * PhysicsBody.Velocity.z / 750f;
		WorldRotation = Rotation.Lerp( WorldRotation, Rotation.LookAt( TargetLookAt, Vector3.Up ), Time.Delta * 10f );
	}

	public void OnCollisionStart( Collision other )
	{
		if ( CollisionSound is not null )
			Sound.Play( CollisionSound, WorldPosition );
		Model.Set( "grounded", true );

		if ( !PhysicsBody.IsValid() )
			return;

		if ( -other.Contact.Normal.z > 0.5f )
			PhysicsBody.Velocity = (Vector3.Up + WorldRotation.Forward) * ProjectileSpeed;

	}

	public void OnCollisionStop( CollisionStop other )
	{
		Model.Set( "grounded", false );
	}
}
