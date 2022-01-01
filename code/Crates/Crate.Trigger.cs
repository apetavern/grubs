using Sandbox;

namespace Grubs.Crates
{
	public partial class Crate
	{
		private class CrateTrigger : ModelEntity
		{
			public override void Spawn()
			{
				base.Spawn();

				SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 32 ) );
				CollisionGroup = CollisionGroup.Trigger;
				Transmit = TransmitType.Never;
			}
		}
	}
}
