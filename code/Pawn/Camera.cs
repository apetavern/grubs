using Sandbox;
using TerryForm.Utils;

namespace TerryForm.Pawn
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

		private Range DistanceRange { get; } = new Range( 512f, 2048f );

		private float Distance { get; set; } = 1024f;
		private float DistanceScrollRate => 32f;
		private bool CenterOnPawn { get; set; } = true;

		public Vector3 Center { get; set; }

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null )
				return;

			// Distance scrolling
			Distance += -Input.MouseWheel * DistanceScrollRate;
			Distance = DistanceRange.Clamp( Distance );

			// If we haven't moved the camera, center it on the pawn
			var cameraCenter = (CenterOnPawn) ? pawn.Position : Center;

			// Lerp to our target position
			var targetPosition = cameraCenter + Vector3.Right * Distance;
			Position = Position.LerpTo( targetPosition, 5 * Time.Delta );

			// Rotate towards the target position
			var lookDir = (cameraCenter - targetPosition).Normal;
			Rotation = Rotation.LookAt( lookDir, Vector3.Up );

			if ( Input.Down( InputButton.Attack2 ) )
				MoveCamera( pawn );

			//
			// Camera properties
			//
			FieldOfView = 65;
			ZNear = 8;
			ZFar = 25000;
			Viewer = null;
		}

		/// <summary>
		/// Handles any camera movement
		/// </summary>
		private void MoveCamera( Entity pawn )
		{
			var delta = new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y );

			if ( CenterOnPawn )
			{
				Center = pawn.Position;

				// Check if we've moved the camera, don't center on the pawn if we have
				if ( !delta.LengthSquared.AlmostEqual( 0, 0.1f ) )
					CenterOnPawn = false;
			}

			Center += delta;
		}
	}
}
