using Sandbox;
using TerryForm.Utils;

namespace TerryForm
{
	/// <summary>
	/// Basic 2.5D camera
	/// </summary>
	public class Camera : Sandbox.Camera
	{
		public override void Activated()
		{
			Position = default;
			Rotation = default;
		}

		private Range DistanceRange { get; } = new Range( 1024, 4096 );

		private float Distance { get; set; } = 1024f;
		private float DistanceScrollRate => 128f;

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			// TODO: Check if the pawn is outside the camera's bounds (w/ a 'safe area') and recenter

			Distance += -Input.MouseWheel * DistanceScrollRate;
			Distance = DistanceRange.Clamp( Distance );

			var targetPosition = pawn.EyePos + Vector3.Right * Distance;
			Position = Position.LerpTo( targetPosition, 5 * Time.Delta );

			var lookDir = (pawn.EyePos - targetPosition).Normal;
			Rotation = Rotation.LookAt( lookDir, Vector3.Up );

			Viewer = pawn;
		}
	}
}
