using Sandbox;
using Sandbox.Utility;

namespace Grubs.Equipment.Tools;

public sealed class JetpackTool : Tool
{
	private float smoothedJetpackDir { get; set; }

	public override void OnHolster()
	{
		if ( Equipment.Grub != null )
			Equipment.Grub.Animator.GrubRenderer.Set( "jetpack_active", false );

		FBFlame.Enabled = false;
		UDFlame1.Enabled = false;
		UDFlame2.Enabled = false;
		IsFiring = false;
	}

	protected override void FireFinished()
	{
		base.FireFinished();
		if ( Equipment.Grub != null )
			Equipment.Grub.Animator.GrubRenderer.Set( "jetpack_active", false );

		FBFlame.Enabled = false;
		UDFlame1.Enabled = false;
		UDFlame2.Enabled = false;
		IsFiring = false;
	}

	[Property] public GameObject FBFlame { get; set; }

	[Property] public GameObject UDFlame1 { get; set; }

	[Property] public GameObject UDFlame2 { get; set; }

	public void AnimateFlames()
	{
		var characterController = Equipment.Grub.CharacterController;

		FBFlame.Enabled = true;
		UDFlame1.Enabled = true;
		UDFlame2.Enabled = true;

		Transform Middle_Flame = Equipment.Model.GetAttachment( "jet_middle" ).Value;
		Transform Left_Flame = Equipment.Model.GetAttachment( "jet_left" ).Value;
		Transform Right_Flame = Equipment.Model.GetAttachment( "jet_right" ).Value;

		FBFlame.Transform.Position = Middle_Flame.Position + characterController.Velocity / 100f;
		FBFlame.Transform.Rotation = Middle_Flame.Rotation;

		FBFlame.Transform.Scale = MathX.Lerp( FBFlame.Transform.Scale.x, MathF.Abs( Input.AnalogMove.y * 0.4f ), Time.Delta * 5f );

		UDFlame1.Transform.Position = Left_Flame.Position + characterController.Velocity / 100f;
		UDFlame1.Transform.Rotation = Left_Flame.Rotation;
		UDFlame2.Transform.Position = Right_Flame.Position + characterController.Velocity / 100f;
		UDFlame2.Transform.Rotation = Right_Flame.Rotation;

		UDFlame1.Transform.Scale = MathX.Lerp( UDFlame1.Transform.Scale.x, MathF.Abs( Input.AnalogMove.x * 0.4f ), Time.Delta * 5f );
		UDFlame2.Transform.Scale = MathX.Lerp( UDFlame2.Transform.Scale.x, MathF.Abs( Input.AnalogMove.x * 0.4f ), Time.Delta * 5f );
	}

	protected override void HandleComplexFiringInput()
	{
		base.HandleComplexFiringInput();

		if ( Input.Pressed( "fire" ) && !IsFiring )
		{
			IsFiring = true;
		}
		else if ( Input.Pressed( "fire" ) && IsFiring )
		{
			FireFinished();
		}

		if ( IsFiring )
		{
			AnimateFlames();
			var characterController = Equipment.Grub.CharacterController;
			var animator = Equipment.Grub.Animator;
			animator.GrubRenderer.Set( "jetpack_active", Equipment.Deployed );
			animator.GrubRenderer.SetBodyGroup( "hide_hands", 0 );

			smoothedJetpackDir = MathX.Lerp( smoothedJetpackDir, Vector3.Dot( new Vector3( -Input.AnalogMove.y, 0, 0 ), characterController.Velocity.Normal ), Time.Delta * 5f );

			if ( Input.AnalogMove.x > 0 && characterController.IsOnGround )
			{
				characterController.ReleaseFromGround();
			}

			if ( !characterController.IsOnGround )
			{
				animator.GrubRenderer.Set( "jetpack_dir", smoothedJetpackDir );
				UpdateRotation();
				characterController.Velocity += new Vector3( -Input.AnalogMove.y, 0, 0.75f + Input.AnalogMove.x * 1.5f ) * 8f;
			}
			else
			{
				animator.GrubRenderer.Set( "jetpack_dir", 0 );
			}
		}
	}

	private void UpdateRotation()
	{
		var characterController = Equipment.Grub.CharacterController;
		Rotation targetRotation = -characterController.Velocity.x switch
		{
			<= -1 => Equipment.Grub.Transform.Rotation.Angles().WithYaw( 0 ),
			>= 1 => Equipment.Grub.Transform.Rotation.Angles().WithYaw( 180 ),
			_ => Equipment.Grub.Transform.Rotation
		};

		Equipment.Grub.Transform.Rotation = Rotation.Lerp( Equipment.Grub.Transform.Rotation, targetRotation, Time.Delta * 5f );
		Equipment.Grub.PlayerController.Facing = targetRotation.y <= 0 ? 1 : -1;
	}
}
