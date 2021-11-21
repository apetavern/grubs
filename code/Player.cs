using Sandbox;

namespace TerryForm
{
	public class Player : Sandbox.Player
	{
		public override void Respawn()
		{
			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
			EnableAllCollisions = false;
		}
	}
}
