using Grubs.Common;
using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Gamemodes;
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
	private RealTimeUntil _timeSinceTargeted;
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

	public void QueueTarget( GameObject targetObject, float duration = 0 )
	{
		var cameraTarget = new CameraTarget() { Object = targetObject, Duration = duration };
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
			
		if ( TargetQueue.Count == 0 )
		{
			GameObject targetObj = null;
			
			if ( BaseGameMode.Current is FreeForAll freeForAllMode )
			{
				targetObj = freeForAllMode.ActivePlayer.ActiveGrub.GameObject;
			}
			else
			{
				targetObj = Player.Local?.ActiveGrub?.GameObject;	
			}
			
			Target = new CameraTarget
			{
				Object = targetObj, 
				Duration = 0f,
			};
			return;
		}

		if ( _timeSinceTargeted > Target.Duration )
		{
			var newTarget = TargetQueue.Dequeue();
			Log.Info( $"Switching camera target to {newTarget.Object.Name}" );
			Target = newTarget;
			_timeSinceTargeted = 0f;
		}
		// var targetGuid = Gamemode.GetCurrent().CameraTarget;
		// if ( targetGuid != Guid.Empty )
		// {
		// 	SetTarget( Scene.Directory.FindByGuid( targetGuid ), .5f );
		// 	return;
		// }
		//
		// foreach ( var projectile in Scene.GetAllComponents<Projectile>().Where( p => p.Active ) )
		// {
		// 	if ( !projectile.IsValid() )
		// 		continue;
		// 	if ( projectile.Tags.Has( "notarget" ) || Resolution.ForceResolved.Contains( projectile.GameObject.Id ) )
		// 		continue;
		//
		// 	SetTarget( projectile.GameObject, 1.5f );
		// 	return;
		// }
		//
		// var component = Scene.Directory.FindComponentByGuid( Gamemode.FFA.ActivePlayerId );
		// if ( component is Player player && player.ActiveGrub.IsValid() )
		// 	SetTarget( player.ActiveGrub.GameObject );
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

		var minX = terrain.WorldTextureLength * -0.5f + padding;
		var maxX = terrain.WorldTextureLength * 0.5f - padding;
		_center.x = _center.x.Clamp( minX, maxX );

		var water = terrain?.Water;
		var minZ = (water?.WorldPosition.z ?? 0) + padding;
		var maxZ = terrain.WorldTextureHeight - padding + 128;
		_center.z = _center.z.Clamp( minZ, maxZ );
	}
}
