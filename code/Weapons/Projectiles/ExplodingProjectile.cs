using Sandbox;
using System.Linq;

namespace TerryForm.Weapons
{
	public partial class ExplodingProjectile : Projectile
	{
		protected float DamageRadius { get; set; } = 20;
		protected int TimesBounced { get; set; }
		protected int MaxBounces { get; set; }

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

		protected void Bounce()
		{
			TimesBounced++;
			// Add some more velocity per bounce?
		}

		protected void Explode()
		{
			ExplodeEffects();

			var playersWithinRadius = Physics.GetEntitiesInSphere( Position, DamageRadius ).OfType<Player>();

			foreach ( var player in playersWithinRadius )
				Log.Info( $"Deal damage to {player.Name}" );

			DebugOverlay.Sphere( Position, DamageRadius, Color.Red, false, 2 );

			Delete();
		}

		public override void OnTick()
		{
			if ( IsServer || IsAuthority )
			{
				var direction = Velocity.Normal;
				Rotation = Rotation.LookAt( direction );
			}
		}

		[ClientRpc]
		public virtual void ExplodeEffects()
		{
			Particles.Create( "particles/explosion_fireball.vpcf", Position );
			_ = new Sandbox.ScreenShake.Perlin( 1f, 1f, 1f, 1f );
		}
	}
}
