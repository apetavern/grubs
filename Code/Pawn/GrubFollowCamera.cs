using Grubs.Common;
using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn;
using Grubs.Terrain;

namespace Grubs.Pawn;

[Title( "Grubs - Follow Camera" ), Category( "Grubs" )]
public sealed class GrubFollowCamera : LocalComponent<GrubFollowCamera>
{
	public float Distance { get; set; } = 1024f;
	public bool AllowZooming { get; set; } = true;
	public bool AutomaticRefocus { get; set; } = true;

	private Queue<CameraTarget> TargetQueue { get; } = new();
	private CameraTarget Target { get; set; }

	private bool _isFocusingTarget;
	private Vector3 _center;
	private Vector3 _panDelta;
	private RealTimeSince _timeSinceTargeted;
	private RealTimeSince _timeSinceMousePan;

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local = this;
	}

	protected override void OnUpdate()
	{
		var listenerTransform = Transform.World;
		listenerTransform.Position = WorldPosition.WithY( 480f );
		Sound.Listener = listenerTransform;

		FindTarget();
		
		if ( Target.Object.IsValid() && _isFocusingTarget )
			_center = Target.Object.WorldPosition;

		ClampCamera();

		var cam = GameObject;
		var targetPos = _center + Vector3.Right * Distance;
		targetPos.z += 32f;
		cam.WorldPosition = cam.WorldPosition.LerpTo( targetPos, Time.Delta * 5f );

		if ( Input.Down( "camera_pan" ) )
			PanCamera();

		var requestRefocus = Input.Pressed( "camera_reset" );
		var automaticRefocus = !Input.Down( "camera_pan" ) && _timeSinceMousePan > 3 && AutomaticRefocus;
		if ( Target.Object.IsValid() && (requestRefocus || automaticRefocus) )
			_isFocusingTarget = true;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( AllowZooming )
		{
			if ( Input.UsingController && Input.Down( "camera_pan" ) )
				Distance -= Input.GetAnalog( InputAnalog.LeftStickY ) * -24f;
			else
				Distance -= Input.MouseWheel.y * 32f;
		}

		Distance = Distance.Clamp( 128f, 2048f );

		AdjustHighlightOutline();
	}

	public void QueueTarget( GameObject targetObject, float duration = 1f )
	{
		if ( TargetQueue.Select( target => target.Object ).Contains( targetObject ) )
		{
			var existingTarget = TargetQueue.First( target => target.Object == targetObject );
			Log.Info( $"Found existing target while queueing: {existingTarget.Object.Name}." );
			existingTarget.Duration = duration;
			return;
		}
		
		var cameraTarget = new CameraTarget { Object = targetObject, Duration = duration };
		Log.Info( $"Target not found, queueing new object: {cameraTarget.Object}." );
		TargetQueue.Enqueue( cameraTarget );
	}

	private void AdjustHighlightOutline()
	{
		var highlights = Scene.GetAllComponents<HighlightOutline>();

		foreach ( var highlight in highlights )
		{
			var desiredWidth = (1 / Distance * 100).Clamp( 0f, 0.35f );
			highlight.Width = highlight.Width.LerpTo( desiredWidth, Time.Delta * 5f );
		}
	}

	private void FindTarget()
	{
		if ( !BaseGameMode.Current.GameStarted )
			return;

		if ( TargetQueue.Count != 0 && (Target.Duration == 0f || _timeSinceTargeted > Target.Duration || !Target.Object.IsValid() ) )
		{
			var newTarget = TargetQueue.Dequeue();
			if ( !newTarget.Object.IsValid() )
				return;

			Target = newTarget;
			_timeSinceTargeted = 0f;
		}

		if ( TargetQueue.Count != 0 || (_timeSinceTargeted <= Target.Duration && Target.Object.IsValid()) ) 
			return;

		var activeProjectile = Scene.GetAllComponents<Projectile>().FirstOrDefault( p => p.Active && !p.Tags.Has( "notarget" ) );
		if ( activeProjectile.IsValid() )
		{
			Target = new CameraTarget { Object = activeProjectile.GameObject, Duration = 0f };
			return;
		}
		
		if ( BaseGameMode.Current is FreeForAll freeForAllMode )
		{
			SetFreeForAllDefaultTarget( freeForAllMode );
			return;
		}
			
		Target = new CameraTarget
		{
			Object = Player.Local?.ActiveGrub?.GameObject, 
			Duration = 0f,
		};
	}

	private void SetFreeForAllDefaultTarget( FreeForAll freeForAll )
	{
		if ( !freeForAll.IsValid() )
			return;

		if ( !freeForAll.ActivePlayer.IsValid() || !freeForAll.ActivePlayer.ActiveGrub.IsValid() )
			return;
		
		var targetObj = freeForAll.ActivePlayer.ActiveGrub.GameObject;
		Target = new CameraTarget { Object = targetObj, Duration = 0f, };
	}

	public void PanCamera()
	{
		_timeSinceMousePan = 0;

		_panDelta = Input.UsingController
			? new Vector3( Input.GetAnalog( InputAnalog.RightStickX ), 0,
				-Input.GetAnalog( InputAnalog.RightStickY ) ) * 4
			: new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y ) * 2;

		if ( _isFocusingTarget )
		{
			_center = Target.Object?.WorldPosition ?? _center;

			if ( !_panDelta.LengthSquared.AlmostEqual( 0, 0.1f ) )
				_isFocusingTarget = false;
		}

		_center += _panDelta;
	}

	private void ClampCamera()
	{
		const float padding = 4;
		var terrain = GrubsTerrain.Instance;
		if ( !terrain.IsValid() )
			return;

		var minX = terrain.WorldTextureLength * -0.5f + padding;
		var maxX = terrain.WorldTextureLength * 0.5f - padding;
		_center.x = _center.x.Clamp( minX, maxX );

		var water = terrain.Water;
		if ( !water.IsValid() )
			return;
		
		var minZ = water.WorldPosition.z + padding;
		var maxZ = terrain.WorldTextureHeight - padding + 128;
		_center.z = _center.z.Clamp( minZ, maxZ );
	}
}
