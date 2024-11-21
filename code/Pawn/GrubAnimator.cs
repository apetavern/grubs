using Grubs.Equipment;
using Grubs.Pawn.Controller;

namespace Grubs.Pawn;

[Title( "Grubs - Animator" ), Category( "Grubs" )]
public sealed class GrubAnimator : Component
{
	[Property] public required Grub Grub { get; set; }
	[Property] public required SkinnedModelRenderer GrubRenderer { get; set; }
	[Property] public required GrubPlayerController Controller { get; set; }

	[Sync] public bool IsOnJetpack { get; set; }
	[Sync] public float JetpackDir { get; set; }

	public bool Thinking { get; set; }

	private float _incline;

	private Vector3 _looktarget;

	protected override void OnUpdate()
	{
		if ( !Grub.IsValid() || !GrubRenderer.IsValid() || !Controller.IsValid() )
			return;

		GrubRenderer.Set( "aimangle", Controller.EyeRotation.Pitch() * -Controller.Facing );
		GrubRenderer.Set( "grounded", Controller.IsGrounded );
		GrubRenderer.Set( "velocity", Controller.Velocity.Length );
		GrubRenderer.Set( "bot_thinking", Thinking );
		GrubRenderer.Set( "heightdiff", Controller.IsOnRope ? 15f : 0f );
		GrubRenderer.Set( "jetpack_active", IsOnJetpack );
		GrubRenderer.Set( "jetpack_dir", MathX.Lerp( GrubRenderer.GetFloat( "jetpack_dir" ), JetpackDir, Time.Delta * 5f ) );

		var holdPose = HoldPose.None;
		if ( Grub.ActiveEquipment.IsValid() && Controller.ShouldShowWeapon() && Grub.IsActive )
			holdPose = Grub.ActiveEquipment.HoldPose;

		GrubRenderer.Set( "holdpose", (int)holdPose );

		var shouldLookAt = Controller.IsGrounded && (Grub.ActiveEquipment is null || !Controller.ShouldShowWeapon())
												 && !GrubRenderer.GetBool( "lowhp" )
												 && !Controller.IsChargingBackflip;

		var shouldHideHands = Controller.Velocity.Length > 0 && !Controller.IsChargingBackflip && !IsOnJetpack;

		GrubRenderer.SetBodyGroup( "hide_hands", shouldHideHands ? 1 : 0 );

		GrubRenderer.Set( "onrope", Grub.PlayerController.IsOnRope );

		GrubRenderer.Set( "lookatweight",
			MathX.Lerp( GrubRenderer.GetFloat( "lookatweight" ), shouldLookAt ? 1f : 0f,
				0.2f ) );

		_looktarget = Vector3.Lerp( _looktarget, new Vector3( 3f, 4f * -Controller.Facing, 0f ), Time.Delta * 5f );

		GrubRenderer.Set( "looktarget", _looktarget );

		var tr = Scene.Trace
			.Ray(
				Controller.WorldPosition
				+ Controller.WorldRotation.Forward * 4f
				+ Controller.WorldRotation.Up * 10f,
				Controller.WorldPosition + Controller.WorldRotation.Down * 128 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		_incline = MathX.Lerp( _incline, Controller.WorldRotation.Forward.Angle( tr.Normal ) - 90f, 0.2f );
		GrubRenderer.Set( "incline", _incline );
		GrubRenderer.Set( "backflip_charge", Controller.BackflipCharge );
		GrubRenderer.Set( "hardfall", Controller.IsHardFalling );

		if ( Grub.Health.IsValid() )
		{
			GrubRenderer.Set( "lowhp", Grub.Health.CurrentHealth <= Grub.Health.MaxHealth / 4f );
			GrubRenderer.Set( "explode", Grub.Health.DeathInvoked );
		}
	}

	[Broadcast]
	public void TriggerJump()
	{
		Sound.Play( "grub_jump", WorldPosition );
	}

	[Broadcast]
	public void TriggerBackflip()
	{
		Sound.Play( "grub_backflip", WorldPosition );
		GrubRenderer.Set( "backflip", true );
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
