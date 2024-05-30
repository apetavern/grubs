using Sandbox;

namespace Grubs;

public sealed class Spinning : Component
{
	[Property] public float SpinSpeed { get; set; } = 1f;
	[Property] public RotationAxis SpinAxis { get; set; } = RotationAxis.Yaw;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		var rotation = Transform.Rotation;

		switch ( SpinAxis )
		{
			case RotationAxis.Pitch:
				rotation *= Rotation.FromPitch( SpinSpeed * Time.Delta );
				break;
			case RotationAxis.Yaw:
				rotation *= Rotation.FromYaw( SpinSpeed * Time.Delta );
				break;
			case RotationAxis.Roll:
				rotation *= Rotation.FromRoll( SpinSpeed * Time.Delta );
				break;
		}

		Transform.Rotation = rotation;
	}
}

public enum RotationAxis
{
	Pitch,
	Yaw,
	Roll
}
