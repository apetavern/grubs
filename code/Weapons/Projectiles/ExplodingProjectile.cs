using Sandbox;
using System.Linq;

namespace TerryForm.Weapons
{
	public partial class ExplodingProjectile : Projectile
	{
		private float DamageRadius { get; set; } = 20;
		private int TimesBounced { get; set; }
		private int MaxBounces { get; set; }

		public ExplodingProjectile WithRadius( float radius )
		{
			DamageRadius = radius;
			return this;
		}

		public ExplodingProjectile SetMaxBounces( int maxBounces )
		{
			MaxBounces = maxBounces;
			return this;
		}

		public async void ExplodeAfterSecondsAsync( float seconds )
		{
			await GameTask.DelaySeconds( seconds );

			Explode();
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( TimesBounced >= MaxBounces || MaxBounces == 0 )
				Explode();
			else
				Bounce();
		}

		private void Bounce()
		{
			TimesBounced++;

			// Add some more velocity per bounce?
		}

		private void Explode()
		{
			ExplodeEffects();

			var playersWithinRadius = Physics.GetEntitiesInSphere( Position, DamageRadius ).OfType<Player>();

			foreach ( var player in playersWithinRadius )
			{
				Log.Info( $"Deal damage to {player.Name}" );
			}

			DebugOverlay.Sphere( Position, DamageRadius, Color.Red, false, 2 );

			Delete();
		}

		[ClientRpc]
		public virtual void ExplodeEffects()
		{
			Particles.Create( "particles/explosion_fireball.vpcf", Position );
		}
	}
}
