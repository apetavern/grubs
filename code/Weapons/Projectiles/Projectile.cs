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

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			base.OnPhysicsCollision( eventData );

			if ( eventData.Entity is Player player )
				Log.Info( $"Deal damage to {player.Name}" );
		}
	}
}
