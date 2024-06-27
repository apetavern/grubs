namespace Grubs.Equipment.Tools;

public sealed class JetpackTool : Tool
{
	/// <summary>
	/// Forward/Back flame
	/// </summary>
	[Property] public GameObject FBFlame { get; set; }

	/// <summary>
	/// Up/Down flame 1
	/// </summary>
	[Property] public GameObject UDFlame1 { get; set; }

	/// <summary>
	/// Up/Down flame 2
	/// </summary>
	[Property] public GameObject UDFlame2 { get; set; }

	[Property] private float MaxJetFuel { get; set; } = 10f;

	[Sync] private float ForwardBackFlameScale { get; set; }
	[Sync] private float UpDownFlameScale { get; set; }
	[Sync] private bool ShouldAnimate { get; set; }

	private float _currentJetFuel;
	private float _jetpackDir;
	private SoundHandle _jetSound;

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Equipment.Grub != null )
			Equipment.Grub.Animator.IsOnJetpack = false;

		FBFlame.Enabled = false;
		UDFlame1.Enabled = false;
		UDFlame2.Enabled = false;
		IsFiring = false;
		_currentJetFuel = MaxJetFuel;
		_jetSound?.Stop();
	}

	protected override void FireFinished()
	{
		base.FireFinished();
		if ( Equipment.Grub != null )
			Equipment.Grub.Animator.IsOnJetpack = false;

		FBFlame.Enabled = false;
		UDFlame1.Enabled = false;
		UDFlame2.Enabled = false;
		IsFiring = false;
		_currentJetFuel = MaxJetFuel;
		_jetSound?.Stop();
	}

	public void AnimateFlames()
	{
		if ( !Equipment.Model.Active )
			return;

		var characterController = Equipment.Grub.CharacterController;

		FBFlame.Enabled = true;
		UDFlame1.Enabled = true;
		UDFlame2.Enabled = true;

		var Middle_Flame = Equipment.Model.GetAttachment( "jet_middle" ).Value;
		var Left_Flame = Equipment.Model.GetAttachment( "jet_left" ).Value;
		var Right_Flame = Equipment.Model.GetAttachment( "jet_right" ).Value;

		FBFlame.Transform.Position = Middle_Flame.Position + characterController.Velocity * Time.Delta;
		FBFlame.Transform.Rotation = Middle_Flame.Rotation;

		if ( !IsProxy )
		{
			ForwardBackFlameScale = MathF.Abs( Equipment.Grub.PlayerController.MoveInput * 0.4f );
			UpDownFlameScale = MathF.Abs( Equipment.Grub.PlayerController.LookInput * 0.4f );
		}

		FBFlame.Transform.Scale = MathX.Lerp( FBFlame.Transform.Scale.x, ForwardBackFlameScale, Time.Delta * 5f );

		UDFlame1.Transform.Position = Left_Flame.Position + characterController.Velocity / 100f;
		UDFlame1.Transform.Rotation = Left_Flame.Rotation;
		UDFlame2.Transform.Position = Right_Flame.Position + characterController.Velocity / 100f;
		UDFlame2.Transform.Rotation = Right_Flame.Rotation;

		UDFlame1.Transform.Scale = MathX.Lerp( UDFlame1.Transform.Scale.x, UpDownFlameScale, Time.Delta * 5f );
		UDFlame2.Transform.Scale = MathX.Lerp( UDFlame2.Transform.Scale.x, UpDownFlameScale, Time.Delta * 5f );

		if ( _jetSound is not null )
			_jetSound.Position = Equipment.Grub.Transform.Position;
	}

	protected override void OnUpdate()
	{
		ShouldAnimate = Equipment.Deployed && IsFiring;
		if ( ShouldAnimate )
			AnimateFlames();

		base.OnUpdate();
	}

	protected override void HandleComplexFiringInput()
	{
		base.HandleComplexFiringInput();

		if ( Input.Pressed( "fire" ) && !IsFiring )
		{
			FireEffects();
			IsFiring = true;
		}

		if ( _currentJetFuel <= 0 && IsFiring )
			FireFinished();

		if ( IsFiring )
		{
			var characterController = Equipment.Grub.CharacterController;
			var animator = Equipment.Grub.Animator;
			animator.IsOnJetpack = true;

			_jetpackDir = Vector3.Dot( new Vector3( -Input.AnalogMove.y, 0, 0 ), characterController.Velocity.Normal );

			_jetSound.Volume = Input.AnalogMove.Length + 0.1f;

			if ( Input.AnalogMove.x > 0 && characterController.IsOnGround )
			{
				characterController.ReleaseFromGround();
			}

			if ( !characterController.IsOnGround )
			{
				_currentJetFuel -= Time.Delta * Input.AnalogMove.Length;
				UpdateRotation();
				characterController.Accelerate( new Vector3( -Input.AnalogMove.y, 0, 0.75f + Input.AnalogMove.x * 1.5f ) * 72f );
				characterController.CurrentGroundAngle = 0;
			}
			else
			{
				_jetpackDir = 0;
			}

			animator.JetpackDir = _jetpackDir;
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

	[Broadcast]
	private void FireEffects()
	{
		_jetSound = Sound.Play( "thrust" );
		_jetSound.Volume = 0.25f;
	}
}
