namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Goat Projectile" ), Category( "Equipment" )]
public class GoatProjectile : Projectile, Component.ICollisionListener
{
	[Property] public bool Droppable { get; set; } = false;
	[Property] public Rigidbody PhysicsBody { get; set; }
	[Property] public bool SetPositionOnStart { get; set; } = true;

	public override bool Resolved => PhysicsBody.Velocity.IsNearlyZero( 0.1f );

	protected override void OnStart()
	{
		if ( Source is null )
			return;

		Transform.Position = Source.GetStartPosition( Droppable );

		if ( Droppable )
			return;

		if ( PlayerController is null )
			return;

		var dir = (PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing) + Vector3.Up;
		Transform.Position += dir * 16f;
		PhysicsBody.ApplyImpulse( dir * Charge * ProjectileSpeed );

		Model.Set( "active", true );
	}

	Vector3 TargetLookAt { get; set; }

	protected override void OnUpdate()
	{
		if ( PlayerController is null )
			return;

		base.OnUpdate();
		Model.Set( "active", true );
		TargetLookAt = (PlayerController.EyeRotation.Forward.Normal.WithZ( 0 ) * PlayerController.Facing) + Vector3.Up * PhysicsBody.Velocity.z / 750f;
		Transform.Rotation = Rotation.Lerp( Transform.Rotation, Rotation.LookAt( TargetLookAt, Vector3.Up ), Time.Delta * 10f );
	}

	public void OnCollisionStart( Collision other )
	{
		Model.Set( "grounded", true );
		if ( -other.Contact.Normal.z > 0.5f )
			PhysicsBody.Velocity = (Vector3.Up + Transform.Rotation.Forward) * ProjectileSpeed;

	}

	public void OnCollisionStop( CollisionStop other )
	{
		Model.Set( "grounded", false );
	}
}
