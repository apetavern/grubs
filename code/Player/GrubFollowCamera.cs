namespace Grubs.Player;

public class GrubFollowCamera : Component
{
	[Property] public required GameObject Target { get; set; }

	protected override void OnUpdate()
	{
		var cam = GameObject;
		var targetPos = Target.Transform.Position;
		targetPos.y = cam.Transform.Position.y;
		targetPos.z += 32f;
		cam.Transform.Position = Vector3.Lerp( cam.Transform.Position, targetPos, 0.1f );
	}
}
