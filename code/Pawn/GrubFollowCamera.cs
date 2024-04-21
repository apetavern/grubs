﻿using Grubs.Equipment.Weapons;
using Grubs.Gamemodes;
using Grubs.Terrain;

namespace Grubs.Pawn;

public class GrubFollowCamera : Component
{
	public static GrubFollowCamera Local { get; set; }
	public float Distance { get; set; } = 1024f;

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
		if ( _timeUntilCameraUnlock )
			FindTarget();

		if ( Target.IsValid() && _isFocusingTarget )
			_center = Target.Transform.Position;

		var cam = GameObject;
		var targetPos = _center + Vector3.Right * Distance;
		targetPos.z += 32f;
		cam.Transform.Position = cam.Transform.Position.LerpTo( targetPos, Time.Delta * 5f );

		if ( Input.Down( "camera_pan" ) )
			PanCamera();

		ClampCamera();

		var requestRefocus = Input.Pressed( "camera_reset" );
		var automaticRefocus = !Input.Down( "camera_pan" ) && _timeSinceMousePan > 3;
		if ( Target.IsValid() && (requestRefocus || automaticRefocus) )
			_isFocusingTarget = true;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Distance -= Input.MouseWheel.y * 32f;
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

		foreach ( var projectile in Scene.GetAllComponents<ProjectileComponent>().Where( p => p.Active ) )
		{
			SetTarget( projectile.GameObject, 2.5f );
			return;
		}

		var component = Scene.Directory.FindComponentByGuid( Gamemode.FFA.ActivePlayerId );
		if ( component is Player player && player.ActiveGrub is not null )
			SetTarget( player.ActiveGrub.GameObject );
	}

	private void PanCamera()
	{
		_timeSinceMousePan = 0;

		_panDelta = new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y ) * 2;
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
		var water = GrubsTerrain.Instance?.Water;
		if ( _panDelta.z > 0f || water is null )
			return;

		const float padding = 4;
		var minZ = water.Transform.Position.z + padding;
		if ( _center.z <= minZ )
			_center.z = minZ;
	}
}
