namespace Grubs;

public class GrubsCamera
{
	private readonly FloatRange _distanceRange = new( 128f, 2048f );
	private readonly float _scrollRate = 32f;
	private readonly float _lerpSpeed = 5f;
	private readonly float _cameraOffset = 32f;
	private readonly int _secondsBeforeCentering = 3;

	public float Distance = 1024f;

	private bool _canScroll = true;
	private bool _isCenteredOnGrub = true;
	private Vector3 _center;
	private TimeSince _timeSinceMousePan;
	private Entity _target;
	private TimeUntil _timeUntilCameraUnlock;

	public void CanScroll( bool toggle )
	{
		_canScroll = toggle;
	}

	public void FrameSimulate( IClient _ )
	{
		if ( _canScroll )
		{
			Distance -= Input.MouseWheel * _scrollRate;
			Distance = _distanceRange.Clamp( Distance );
		}

		// Lets clean this up in the future.
		if ( GamemodeSystem.Instance is FreeForAll ffa )
		{
			if ( !ffa.Started )
			{
				var pregameCameraCenter = Vector3.Zero - Vector3.Up * GrubsConfig.TerrainHeight / 2f;

				var pregameTargetPosition = pregameCameraCenter + Vector3.Right * Distance;
				var pregameCurrentPosition = Camera.Position;
				Camera.Position = pregameCurrentPosition.LerpTo( pregameTargetPosition, Time.Delta * _lerpSpeed );

				var pregameLookDir = (pregameCameraCenter - pregameTargetPosition).Normal;
				Camera.Rotation = Rotation.LookAt( pregameLookDir, Vector3.Up );
			}
		}

		if ( _timeUntilCameraUnlock <= 0 )
			FindTarget();

		if ( !_target.IsValid() )
			return;

		var cameraCenter = _isCenteredOnGrub ? _target.Position : _center;
		cameraCenter += Vector3.Up * _cameraOffset;

		var targetPosition = cameraCenter + Vector3.Right * Distance;
		var currentPosition = Camera.Position;
		Camera.Position = currentPosition.LerpTo( targetPosition, Time.Delta * _lerpSpeed );

		var lookDir = (cameraCenter - targetPosition).Normal;
		Camera.Rotation = Rotation.LookAt( lookDir, Vector3.Up );

		if ( Input.Down( InputButton.SecondaryAttack ) )
			MoveCamera();

		if ( Input.Pressed( InputButton.Zoom ) || !Input.Down( InputButton.SecondaryAttack ) && _timeSinceMousePan > _secondsBeforeCentering )
			_isCenteredOnGrub = true;
	}

	public void SetTarget( Entity entity, float duration = 0 )
	{
		if ( entity == _target )
			return;

		if ( duration > 0 )
			_timeUntilCameraUnlock = duration;

		_target = entity;
	}

	private void FindTarget()
	{
		if ( GamemodeSystem.Instance is not Gamemode gm )
			return;

		if ( gm.CameraTarget is not null )
		{
			SetTarget( gm.CameraTarget, 1f );
			return;
		}

		if ( gm.TurnIsChanging )
		{
			foreach ( var grub in Entity.All.OfType<Grub>() )
			{
				if ( grub.LifeState != LifeState.Dying )
					continue;

				if ( grub.LifeState is LifeState.Dead )
					continue;

				SetTarget( grub );
			}

			return;
		}

		foreach ( var gadget in Entity.All.OfType<Gadget>() )
		{
			if ( gadget.ShouldCameraFollow )
			{
				SetTarget( gadget );
				return;
			}
		}

		SetTarget( gm?.ActivePlayer?.ActiveGrub );
		return;
	}

	private void MoveCamera()
	{
		_timeSinceMousePan = 0;

		var delta = new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y ) * 2;
		if ( _isCenteredOnGrub )
		{
			_center = _target.Position;

			// Check if we've moved the camera, don't center on the pawn if we have
			if ( !delta.LengthSquared.AlmostEqual( 0, 0.1f ) )
				_isCenteredOnGrub = false;
		}

		_center += delta;
	}
}
