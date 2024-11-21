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

	[Sync] private float Volume { get; set; }
	[Sync] private float ForwardBackFlameScale { get; set; }
	[Sync] private float UpDownFlameScale { get; set; }
	[Sync] private bool ShouldAnimate { get; set; }

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
		StopEffects();
	}

	protected override void FireFinished()
	{
		base.FireFinished();

		if ( Equipment.Grub != null )
			Equipment.Grub.Animator.IsOnJetpack = false;

		FBFlame.Enabled = false;
		UDFlame1.Enabled = false;
		UDFlame2.Enabled = false;
		StopEffects();
	}

	public void AnimateFlames()
	{
		if ( !Equipment.IsValid() 
		     || !Equipment.Grub.IsValid() 
		     || !Equipment.Grub.CharacterController.IsValid()
		     || !Equipment.Grub.PlayerController.IsValid() )
			return;
		
		if ( !Equipment.Model.Active )
			return;

		_jetSound.Volume = Volume;

		var characterController = Equipment.Grub.CharacterController;

		FBFlame.Enabled = true;
		UDFlame1.Enabled = true;
		UDFlame2.Enabled = true;

		var middleFlame = GetAttachmentOrDefault( "jet_middle" );
		var leftFlame = GetAttachmentOrDefault( "jet_left" );
		var rightFlame = GetAttachmentOrDefault( "jet_right" );

		FBFlame.WorldPosition = middleFlame.Position + characterController.Velocity * Time.Delta;
		FBFlame.WorldRotation = middleFlame.Rotation;

		if ( !IsProxy )
		{
			ForwardBackFlameScale = MathF.Abs( Equipment.Grub.PlayerController.MoveInput * 0.4f );
			UpDownFlameScale = MathF.Abs( Equipment.Grub.PlayerController.LookInput * 0.4f );
		}

		FBFlame.WorldScale = MathX.Lerp( FBFlame.WorldScale.x, ForwardBackFlameScale, Time.Delta * 5f );

		UDFlame1.WorldPosition = leftFlame.Position + characterController.Velocity / 100f;
		UDFlame1.WorldRotation = leftFlame.Rotation;
		UDFlame2.WorldPosition = rightFlame.Position + characterController.Velocity / 100f;
		UDFlame2.WorldRotation = rightFlame.Rotation;

		UDFlame1.WorldScale = MathX.Lerp( UDFlame1.WorldScale.x, UpDownFlameScale, Time.Delta * 5f );
		UDFlame2.WorldScale = MathX.Lerp( UDFlame2.WorldScale.x, UpDownFlameScale, Time.Delta * 5f );

		if ( _jetSound is not null )
			_jetSound.Position = Equipment.Grub.WorldPosition;
	}

	private Transform GetAttachmentOrDefault( string name )
	{
		if ( !Equipment.Model.IsValid() )
			return Transform.World;
		
		var transformNullable = Equipment.Model.GetAttachment( name );
		return transformNullable ?? Transform.World;
	}

	protected override void OnUpdate()
	{
		if ( !Equipment.IsValid() )
			return;
		
		ShouldAnimate = Equipment.Deployed && IsFiring;
		if ( ShouldAnimate )
			AnimateFlames();

		if ( TimesUsed >= MaxUses && IsFiring )
			FireFinished();

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

		if ( IsFiring )
		{
			var characterController = Equipment.Grub.CharacterController;
			var animator = Equipment.Grub.Animator;
			animator.IsOnJetpack = true;

			_jetpackDir = Vector3.Dot( new Vector3( -Input.AnalogMove.y, 0, 0 ), characterController.Velocity.Normal );
			Volume = (Input.AnalogMove.Length + 0.1f) * 0.5f;

			if ( Input.AnalogMove.x > 0 && characterController.IsOnGround )
			{
				characterController.ReleaseFromGround();
			}

			if ( !characterController.IsOnGround )
			{
				TimesUsed += Time.Delta * Input.AnalogMove.Length;
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
			<= -1 => Equipment.Grub.WorldRotation.Angles().WithYaw( 0 ),
			>= 1 => Equipment.Grub.WorldRotation.Angles().WithYaw( 180 ),
			_ => Equipment.Grub.WorldRotation
		};

		Equipment.Grub.WorldRotation = Rotation.Lerp( Equipment.Grub.WorldRotation, targetRotation, Time.Delta * 5f );
		Equipment.Grub.PlayerController.Facing = targetRotation.y <= 0 ? 1 : -1;
	}

	[Broadcast]
	private void FireEffects() => _jetSound = Sound.Play( "thrust" );

	[Broadcast]
	private void StopEffects() => _jetSound?.Stop();
}
