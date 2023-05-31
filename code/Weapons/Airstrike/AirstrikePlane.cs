namespace Grubs;

[Prefab]
public partial class AirstrikePlane : AnimatedEntity
{
	public Prefab Payload { get; set; }

	public int PayloadCount { get; set; }

	public Vector3 TargetPosition { get; set; }
}
