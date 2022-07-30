namespace Grubs.Player;

public class GrubsCamera : CameraMode
{
	private float Distance { get; set; } = 1024;
	private float DistanceScrollRate { get; set; } = 32f;
	private float MinDistance { get; set; } = 128f;
	private float MaxDistance { get; set; } = 2048f;

	private float LerpSpeed { get; set; } = 5f;
	private bool FocusTarget { get; set; } = true;
	private Vector3 Center { get; set; }

	public Entity Target { get; set; }

	public override void Update()
	{
		if ( Target == null )
			return;

		Distance -= Input.MouseWheel * DistanceScrollRate;
		Distance = Distance.Clamp( MinDistance, MaxDistance );

		var cameraCenter = (FocusTarget) ? Target.Position : Center;

		var targetPosition = cameraCenter + Vector3.Right * Distance;
		Position = Position.LerpTo( targetPosition, Time.Delta * LerpSpeed );

		var lookDir = (cameraCenter - targetPosition).Normal;
		Rotation = Rotation.LookAt( lookDir, Vector3.Up );
	}
}
