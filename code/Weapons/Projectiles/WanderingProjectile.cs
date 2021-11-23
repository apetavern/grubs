using Sandbox;

namespace TerryForm.Weapons
{
	public class WanderingProjectile : ExplodingProjectile
	{
		protected bool ShouldWander { get; set; }

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( !ShouldWander )
			{
				// Clear all velocity
				PhysicsBody.Velocity = Vector3.Zero;

				// Begin wandering
				ShouldWander = true;

				return;
			}

			if ( eventData.Entity is Player )
				Explode();
		}

		[Event.Tick.Server]
		public void DoWander()
		{
			if ( !ShouldWander )
				return;

			Log.Info( "Wandering" );
		}
	}
}
