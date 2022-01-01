using Sandbox;
using Grubs.Utils;

namespace Grubs.Pawn
{
	/// <summary>
	/// Basic 2.5D camera
	/// </summary>
	public class Camera : Sandbox.Camera
	{
		public Range DistanceRange { get; } = new Range( 512f, 2048f );
		public float Distance { get; set; } = 1024f;
		private float DistanceScrollRate => 32f;

		private TimeSince TimeSinceMousePan { get; set; }
		private int SecondsBeforeReturnFromPan => 3;

		private bool CenterOnPawn { get; set; } = true;

		public Vector3 Center { get; set; }

		protected Entity LookTarget { get; private set; }

		public override void Activated()
		{
			Position = Vector3.Right * Distance;
			Rotation = Rotation.FromYaw( 90 );
		}

		public void SetLookTarget( Entity target )
		{
			LookTarget = target;
		}

		public override void Update()
		{
			var pawn = LookTarget;

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

			// Check the last time we panned the camera, update CenterOnPawn if greater than N.
			if ( !Input.Down( InputButton.Attack2 ) && TimeSinceMousePan > SecondsBeforeReturnFromPan )
				CenterOnPawn = true;

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
			TimeSinceMousePan = 0;

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
