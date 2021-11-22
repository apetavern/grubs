using Sandbox;
using System.Linq;

namespace TerryForm.Weapons
{
	public partial class ExplodingProjectile : Projectile
	{
		private float damageRadius { get; set; } = 5;

		public ExplodingProjectile WithRadius( float radius )
		{
			damageRadius = radius;
			return this;
		}

		public async void ExplodeAfterSecondsAsync( float seconds )
		{
			await GameTask.DelaySeconds( seconds );

			Explode();
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			Explode();
		}

		private void Explode()
		{
			ExplodeEffects();

			var playersWithinRadius = Physics.GetEntitiesInSphere( Position, damageRadius ).OfType<Player>();

			foreach ( var player in playersWithinRadius )
			{
				Log.Info( $"Deal damage to {player.Name}" );
			}

			DebugOverlay.Sphere( Position, damageRadius, Color.Red, false, 2 );
		}

		[ClientRpc]
		public virtual void ExplodeEffects() { }
	}
}
