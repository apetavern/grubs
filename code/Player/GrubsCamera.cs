namespace Grubs.Player;

public class GrubsCamera : CameraMode
{
	public float Distance { get; set; } = 1024;
	public float DistanceScrollRate { get; set; } = 32f;
	public float MinDistance { get; set; } = 128f;
	public float MaxDistance { get; set; } = 2048f;

	private float LerpSpeed { get; set; } = 5f;
	private bool FocusTarget { get; set; } = true;
	private Vector3 Center { get; set; }
	private float CameraUpOffset { get; set; } = 32f;

	public Entity Target { get; set; }

	public override void Update()
	{
		if ( Target == null )
			return;

		Distance -= Input.MouseWheel * DistanceScrollRate;
		Distance = Distance.Clamp( MinDistance, MaxDistance );

		// Get the center position, plus move the camera up a little bit.
		var cameraCenter = (FocusTarget) ? Target.Position : Center;
		cameraCenter += Vector3.Up * CameraUpOffset;

		var targetPosition = cameraCenter + Vector3.Right * Distance;
		Position = Position.LerpTo( targetPosition, Time.Delta * LerpSpeed );

		var lookDir = (cameraCenter - targetPosition).Normal;
		Rotation = Rotation.LookAt( lookDir, Vector3.Up );
	}
}
