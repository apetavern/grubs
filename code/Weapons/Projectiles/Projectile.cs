using Sandbox;
using System.Threading;

namespace TerryForm.Weapons
{
	public partial class Projectile : ModelEntity
	{
		public override void Spawn()
		{
			CollisionGroup = CollisionGroup.Prop;
			AddCollisionLayer( CollisionLayer.PhysicsProp );

			base.Spawn();
		}

		public Projectile SpawnAt( Vector3 position )
		{
			Position = position;
			ResetInterpolation();

			return this;
		}

		public Projectile WithModel( string modelPath )
		{
			SetModel( modelPath );
			return this;
		}

		public Projectile AddVelocity( Vector3 velocity )
		{
			PhysicsBody.ApplyForce( velocity );
			return this;
		}

		public Projectile AddVelocityAt( Vector3 position, Vector3 velocity )
		{
			PhysicsBody.ApplyForceAt( position, velocity );
			return this;
		}

		public Projectile FireFrom( Vector3 position, Vector3 velocity )
		{
			var projectile = SpawnAt( position );
			projectile.AddVelocity( velocity );

			return this;
		}

		public async void ExplodeAfterSecondsAsync( float seconds )
		{
			await GameTask.DelaySeconds( seconds );

			Explode();
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			base.OnPhysicsCollision( eventData );

			// Do some fun stuff.
		}

		public virtual void Explode()
		{
			ExplodeEffects();

			/*
			 * Deal damage etc here, for example you could find ents within a radius, check the line of sight
			 * from the projectile and damage them if visible.
			 */
		}

		[ClientRpc]
		public virtual void ExplodeEffects() { }
	}
}
