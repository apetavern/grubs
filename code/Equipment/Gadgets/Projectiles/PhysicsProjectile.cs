namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Physics Projectile" ), Category( "Equipment" )]
public class PhysicsProjectile : Projectile
{
	[Property] public bool Droppable { get; set; } = false;
	[Property] public required Rigidbody PhysicsBody { get; set; }
	[Property] public bool SetPositionOnStart { get; set; } = true;

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
			Transform.Position += dir * 16f;
			PhysicsBody.ApplyImpulseAt( PhysicsBody.Transform.Position + Vector3.Up * 0.5f,
				dir * Charge * ProjectileSpeed );
		}
	}
}
