using Grubs.Common;
using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Gamemodes;
using Grubs.Terrain;

namespace Grubs.Pawn;

[Title( "Grubs - Follow Camera" ), Category( "Grubs" )]
public class GrubFollowCamera : Component
{
	public static GrubFollowCamera Local { get; set; }
	public float Distance { get; set; } = 1024f;
	public bool AllowZooming { get; set; } = true;
	public bool AutomaticRefocus { get; set; } = true;

	[Property, ReadOnly] private GameObject Target { get; set; }

	private bool _isFocusingTarget;
	private Vector3 _center;
	private Vector3 _panDelta;
	private RealTimeUntil _timeUntilCameraUnlock;
	private RealTimeSince _timeSinceMousePan;

	public GrubFollowCamera()
	{
		Local = this;
	}

	protected override void OnUpdate()
	{
		var listenerTransform = Transform.World;
		listenerTransform.Position = Transform.Position.WithY( 480f );
		Sound.Listener = listenerTransform;

		if ( _timeUntilCameraUnlock )
			FindTarget();

		if ( Target.IsValid() && _isFocusingTarget )
			_center = Target.Transform.Position;

		ClampCamera();

		var cam = GameObject;
		var targetPos = _center + Vector3.Right * Distance;
		targetPos.z += 32f;
		cam.Transform.Position = cam.Transform.Position.LerpTo( targetPos, Time.Delta * 5f );

		if ( Input.Down( "camera_pan" ) )
			PanCamera();

		var requestRefocus = Input.Pressed( "camera_reset" );
		var automaticRefocus = !Input.Down( "camera_pan" ) && _timeSinceMousePan > 3 && AutomaticRefocus;
		if ( Target.IsValid() && (requestRefocus || automaticRefocus) )
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

	public void SetTarget( GameObject target, float duration = 0 )
	{
		if ( duration > 0 )
			_timeUntilCameraUnlock = duration;

		Target = target;
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
		var targetGuid = Gamemode.Current.CameraTarget;
		if ( targetGuid != Guid.Empty )
		{
			SetTarget( Scene.Directory.FindByGuid( targetGuid ), .5f );
			return;
		}

		foreach ( var projectile in Scene.GetAllComponents<Projectile>().Where( p => p.Active ) )
		{
			if ( projectile.Tags.Has( "notarget" ) || Resolution.ForceResolved.Contains( projectile.GameObject.Id ) )
				continue;

			SetTarget( projectile.GameObject, 1.5f );
			return;
		}

		var component = Scene.Directory.FindComponentByGuid( Gamemode.FFA.ActivePlayerId );
		if ( component is Player player && player.ActiveGrub is not null )
			SetTarget( player.ActiveGrub.GameObject );
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
			_center = Target?.Transform.Position ?? _center;

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
		var minZ = (water?.Transform.Position.z ?? 0) + padding;
		var maxZ = (terrain.WorldTextureHeight - padding) + 128;
		_center.z = _center.z.Clamp( minZ, maxZ );
	}
}
