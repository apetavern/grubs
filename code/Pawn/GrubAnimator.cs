using Grubs.Equipment;
using Grubs.Pawn.Controller;

namespace Grubs.Pawn;

[Title( "Grubs - Animator" ), Category( "Grubs" )]
public sealed class GrubAnimator : Component
{
	[Property] public required Grub Grub { get; set; }
	[Property] public required SkinnedModelRenderer GrubRenderer { get; set; }
	[Property] public required GrubPlayerController Controller { get; set; }

	public bool Thinking { get; set; }

	private float _incline;

	private Vector3 _looktarget;

	protected override void OnUpdate()
	{
		GrubRenderer.Set( "aimangle", Controller.EyeRotation.Pitch() * -Controller.Facing );
		GrubRenderer.Set( "grounded", Controller.IsGrounded );
		GrubRenderer.Set( "velocity", Controller.Velocity.Length );
		GrubRenderer.Set( "bot_thinking", Thinking );

		var holdPose = HoldPose.None;
		if ( Grub.ActiveEquipment is not null && Controller.ShouldShowWeapon() && Grub.IsActive )
			holdPose = Grub.ActiveEquipment.HoldPose;

		GrubRenderer.Set( "holdpose", (int)holdPose );

		var shouldLookAt = Controller.IsGrounded && (Grub.ActiveEquipment is null || !Controller.ShouldShowWeapon())
		                                         && !GrubRenderer.GetBool( "lowhp" )
		                                         && !Controller.IsChargingBackflip;

		var shouldHideHands = Controller.Velocity.Length > 0 && !Controller.IsChargingBackflip;

		GrubRenderer.SetBodyGroup( "hide_hands", shouldHideHands ? 1 : 0 );

		GrubRenderer.Set( "lookatweight",
			MathX.Lerp( GrubRenderer.GetFloat( "lookatweight" ), shouldLookAt ? 1f : 0f,
				0.2f ) );

		_looktarget = Vector3.Lerp( _looktarget, new Vector3( 3f, 4f * -Controller.Facing, 0f ), Time.Delta * 5f );

		GrubRenderer.Set( "looktarget", _looktarget );

		var tr = Scene.Trace
			.Ray(
				Controller.Transform.Position
				+ Controller.Transform.Rotation.Forward * 4f
				+ Controller.Transform.Rotation.Up * 10f,
				Controller.Transform.Position + Controller.Transform.Rotation.Down * 128 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		_incline = MathX.Lerp( _incline, Controller.Transform.Rotation.Forward.Angle( tr.Normal ) - 90f, 0.2f );
		GrubRenderer.Set( "incline", _incline );
		GrubRenderer.Set( "backflip_charge", Controller.BackflipCharge );
		GrubRenderer.Set( "hardfall", Controller.IsHardFalling );
	}

	[Broadcast]
	public void Fire()
	{
		GrubRenderer.Set( "fire", true );
	}

	[Broadcast]
	public void Punch( int value )
	{
		GrubRenderer.Set( "punch_combo", value );
	}
}
