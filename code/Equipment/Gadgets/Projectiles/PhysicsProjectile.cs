﻿namespace Grubs.Equipment.Gadgets.Projectiles;

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
		if ( !Source.IsValid() || !PlayerController.IsValid() )
			return;

		if ( SetPositionOnStart )
		{
			WorldPosition = Source.GetStartPosition( Droppable );

			if ( Droppable )
				return;

			var dir = PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing;
			dir *= Rotation.FromPitch( Game.Random.Float( -DirectionRandomizer, DirectionRandomizer ) );
			WorldPosition += dir * 16f;

			if ( !PhysicsBody.IsValid() )
				return;
			PhysicsBody.ApplyImpulseAt( PhysicsBody.WorldPosition + Vector3.Up * 0.5f,
				dir * Charge * ProjectileSpeed );
		}

		if ( SetRotationOnStart )
		{
			var dir = PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing;
			WorldRotation = Rotation.LookAt( dir, Vector3.Up );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !PhysicsBody.IsValid() )
			return;
		
		var velocity = PhysicsBody.Velocity;
		if ( velocity.Length <= 0.1f )
			return;

		var targetRotation = Rotation.LookAt( velocity.Normal, Vector3.Up );
		const float smoothTime = 0.5f;

		var rotationVelocity = new Vector3();
		var newRotation = Rotation.SmoothDamp(
			PhysicsBody.WorldRotation, targetRotation, ref rotationVelocity, smoothTime, Time.Delta );
		
		PhysicsBody.GameObject.WorldRotation = newRotation;
	}
}
