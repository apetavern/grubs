using Grubs.Player.Controller;

namespace Grubs.Player;

[Title( "Grubs - Animator" )]
[Category( "Grubs" )]
public sealed class GrubAnimator : Component
{
	[Property] public required SkinnedModelRenderer Grub { get; set; }
	[Property] public required GrubPlayerController Controller { get; set; }

	private float _incline;

	private Vector3 _looktarget;

	protected override void OnUpdate()
	{
		Grub.Set( "aimangle", Controller.EyeRotation.Pitch() * -Controller.Facing );
		Grub.Set( "grounded", Controller.IsGrounded );
		Grub.Set( "holdpose", 0 );
		Grub.Set( "velocity", Controller.Velocity.Length );

		Grub.Set( "lookatweight",
			MathX.Lerp( Grub.GetFloat( "lookatweight" ), Controller.IsGrounded && !Grub.GetBool( "lowhp" ) ? 1f : 0f,
				0.2f ) );

		_looktarget = Vector3.Lerp( _looktarget, new Vector3( 3f, 4f * -Controller.Facing, 0f ), Time.Delta * 5f );

		Grub.Set( "looktarget", _looktarget );

		var tr = Scene.Trace
			.Ray(
				Controller.Transform.Position + Controller.Transform.Rotation.Up * 10f,
				Controller.Transform.Position + Controller.Transform.Rotation.Down * 128 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		_incline = MathX.Lerp( _incline, Controller.Transform.Rotation.Forward.Angle( tr.Normal ) - 90f, 0.2f );
		Grub.Set( "incline", _incline );
		Grub.Set( "backflip_charge", Controller.BackflipCharge );
	}
}
