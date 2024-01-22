using Grubs.Player.Controller;

namespace Grubs.Player;

[Title( "Grubs - Animator" )]
[Category( "Grubs" )]
public sealed class GrubAnimator : Component
{
	[Property] public required SkinnedModelRenderer Grub { get; set; }
	[Property] public required GrubPlayerController Controller { get; set; }

	private float _incline;

	protected override void OnUpdate()
	{
		Grub.Set( "aimangle", Controller.EyeRotation.Pitch() * -Controller.Facing );
		Grub.Set( "grounded", Controller.IsGrounded );
		Grub.Set( "holdpose", 0 );
		Grub.Set( "velocity", Controller.Velocity.Length );

		_incline = MathX.Lerp(
			_incline,
			Grub.Transform.Rotation.Forward.Angle( Controller.CurrentGroundAngle ) - 90f,
			0.25f );
		Grub.Set( "incline", _incline );
	}
}
