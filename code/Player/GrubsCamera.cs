namespace Grubs;

public class GrubsCamera
{
	public float Distance = 1024f;
	public bool CanScroll { get; set; } = true;
	public bool AutomaticRefocus { get; set; } = true;

	private readonly FloatRange _distanceRange = new( 128f, 2048f );
	private readonly float _scrollRate = 32f;
	private readonly float _lerpSpeed = 5f;
	private readonly float _cameraOffset = 32f;
	private readonly int _secondsBeforeCentering = 3;

	private bool _isCenteredOnGrub = true;
	private Vector3 _center;
	private Vector3 _panDelta;
	private TimeSince _timeSinceMousePan;
	private Entity _target;
	private RealTimeUntil _timeUntilCameraUnlock;

	private Terrain Terrain => GrubsGame.Instance.Terrain;

	public void FrameSimulate( IClient _ )
	{
		Sound.Listener = new Transform( Camera.Position, Camera.Rotation );

		if ( CanScroll )
		{
			Distance -= Input.MouseWheel * _scrollRate;
			Distance = _distanceRange.Clamp( Distance );
		}

		if ( _timeUntilCameraUnlock )
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

		ClampCamera();

		var requestRefocus = Input.Pressed( InputAction.CameraReset );
		var automaticRefocus = !Input.Down( InputAction.CameraPan ) && _timeSinceMousePan > _secondsBeforeCentering && AutomaticRefocus;

		if ( requestRefocus || automaticRefocus )
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
			foreach ( var grub in Entity.All.OfType<Grub>().Where( e => !e.IsDormant ) )
			{
				if ( grub.LifeState != LifeState.Dying )
					continue;

				if ( grub.LifeState is LifeState.Dead )
					continue;

				SetTarget( grub );
			}

			return;
		}

		foreach ( var gadget in Entity.All.OfType<Gadget>().Where( e => !e.IsDormant ) )
		{
			if ( gadget.ShouldCameraFollow )
			{
				SetTarget( gadget, 3f );
				return;
			}
		}

		SetTarget( gm?.ActivePlayer?.ActiveGrub );
		return;
	}

	private void MoveCamera()
	{
		_timeSinceMousePan = 0;

		_panDelta = new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y ) * 2;
		if ( _isCenteredOnGrub )
		{
			_center = _target.Position;

			// Check if we've moved the camera, don't center on the pawn if we have
			if ( !_panDelta.LengthSquared.AlmostEqual( 0, 0.1f ) )
				_isCenteredOnGrub = false;
		}

		_center += _panDelta;
	}

	private void ClampCamera()
	{
		if ( _panDelta.z > 0f || Terrain.KillZone is null )
			return;

		const float padding = 4;
		var killZoneZ = Terrain.KillZone.Position.z + Terrain.KillZone.CollisionBounds.Maxs.z + padding;
		if ( _center.z <= killZoneZ )
			_center.z = killZoneZ;
	}
}
