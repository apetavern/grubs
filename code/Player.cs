using Sandbox;

namespace TerryForm
{
	public class Player : Sandbox.Player
	{
		public override void Respawn()
		{
			// Need a custom controller!!
			Controller = new WalkController();
			Camera = new Camera();

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			DebugOverlay.Sphere( EyePos, 32f, Color.Yellow, false ); // Visualise the pawn since we don't have a model for it yet
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
			EnableAllCollisions = false;
		}
	}
}
