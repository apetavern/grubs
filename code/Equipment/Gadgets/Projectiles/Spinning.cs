using Sandbox;

namespace Grubs;

public sealed class Spinning : Component
{
	[Property] public Angles RotationSpeed { get; set; }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Transform.Rotation *= RotationSpeed * Time.Delta;
	}
}
