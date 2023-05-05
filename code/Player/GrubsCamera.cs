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
	private RealTimeUntil _timeUntilCameraUnlock;

	private World World => GamemodeSystem.Instance.GameWorld;

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

		if ( _timeUntilCameraUnlock <= 0 )
			FindTarget();

		if ( !_target.IsValid() )
			return;

		var cameraCenter = _isCenteredOnGrub ? _target.Position : _center;
		cameraCenter += Vector3.Up * _cameraOffset;

		var targetPosition = cameraCenter + Vector3.Right * Distance;
		var currentPosition = Camera.Position;
		Camera.Position = currentPosition.LerpTo( targetPosition, Time.Delta * Math.Max( _lerpSpeed, _target.Velocity.Length / 50 ) );

		var lookDir = (cameraCenter - targetPosition).Normal;
		Camera.Rotation = Rotation.LookAt( lookDir, Vector3.Up );

		if ( Input.Down( InputAction.CameraPan ) )
			MoveCamera();

		if ( Input.Pressed( InputAction.CameraReset ) || !Input.Down( InputAction.CameraPan ) && _timeSinceMousePan > _secondsBeforeCentering )
			_isCenteredOnGrub = true;
	}

	public void SetTarget( Entity entity, float duration = 0 )
	{
		if ( duration > 0 )
			_timeUntilCameraUnlock = duration;

		if ( entity == _target )
			return;

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
				SetTarget( gadget, 2f );
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
