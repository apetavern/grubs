namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Physics Projectile" ), Category( "Equipment" )]
public class PhysicsProjectile : Projectile
{
	[Property] public bool Droppable { get; set; } = false;
	[Property] public required Rigidbody PhysicsBody { get; set; }
	[Property] public bool SetPositionOnStart { get; set; } = true;
	[Property] public bool SetRotationOnStart { get; set; } = false;
	[Property] public float DirectionRandomizer { get; set; } = 0f;

	public override bool Resolved => PhysicsBody.Velocity.IsNearlyZero( 0.1f );

	protected override void OnStart()
	{
		if ( Source is null )
			return;

		if ( SetPositionOnStart )
		{
			Transform.Position = Source.GetStartPosition( Droppable );

			if ( Droppable )
				return;

			if ( PlayerController is null )
				return;

			var dir = PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing;

			dir *= Rotation.FromPitch( Game.Random.Float( -DirectionRandomizer, DirectionRandomizer ) );

			Transform.Position += dir * 16f;
			PhysicsBody.ApplyImpulseAt( PhysicsBody.Transform.Position + Vector3.Up * 0.5f,
				dir * Charge * ProjectileSpeed );
		}

		if ( SetRotationOnStart )
		{
			var dir = PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing;
			Transform.Rotation = Rotation.LookAt( dir, Vector3.Up );
		}
	}
}
