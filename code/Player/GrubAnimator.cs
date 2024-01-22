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

		var tr = Scene.Trace.Ray( Controller.Transform.Position + Controller.Transform.Rotation.Up * 10f,
				Controller.Transform.Position + Controller.Transform.Rotation.Down * 128 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		_incline = MathX.Lerp( _incline, Controller.Transform.Rotation.Forward.Angle( tr.Normal ) - 90f, 0.2f );
		Grub.Set( "incline", _incline );
	}
}
