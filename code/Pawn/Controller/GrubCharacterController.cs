﻿using Grubs.Helpers;
using Sandbox.Utility;

namespace Grubs.Pawn.Controller;

[Title( "Grubs - Character Controller" ), Category( "Grubs" ), Icon( "directions_walk", "red", "white" ),
 EditorHandle( "materials/gizmo/charactercontroller.png" )]
public class GrubCharacterController : Component
{
	private int _stuckTries;


	public Vector3 LastVelocity;
	[Range( 0, 200 ), Property] public float Radius { get; set; } = 16.0f;

	[Range( 0, 200 ), Property] public float Height { get; set; } = 64.0f;

	[Range( 0, 50 ), Property] public float StepHeight { get; set; } = 18.0f;

	[Range( 0, 90 ), Property] public float GroundAngle { get; set; } = 45.0f;

	[Range( 0, 20 ), Property] public float Acceleration { get; set; } = 10.0f;

	[Property] public required GrubPlayerController Controller { get; set; }

	[Property] public required ParticleSystem LandingParticles { get; set; }

	[Property] public TagSet IgnoreLayers { get; set; } = new();

	public BBox BoundingBox => new(new Vector3( -Radius, -Radius, 0 ), new Vector3( Radius, Radius, Height ));

	[Sync] public Vector3 Velocity { get; set; }

	[Sync] public bool IsOnGround { get; set; }

	[Sync] public float CurrentGroundAngle { get; set; }

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineBBox( BoundingBox );
	}

	/// <summary>
	/// Add acceleration to the current velocity.
	/// No need to scale by time delta - it will be done inside.
	/// </summary>
	public void Accelerate( Vector3 vector )
	{
		if ( vector.IsNearZeroLength )
			return;

		var wishdir = vector.Normal;
		var wishspeed = vector.Length;

		// See if we are changing direction a bit
		var currentspeed = Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = Acceleration * Time.Delta * wishspeed;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
		var angleFactor = Easing.ExpoOut( 1.0f - -Controller.Facing * CurrentGroundAngle / 80f ).Clamp( 0f, 1f );
		Velocity *= angleFactor;
	}

	/// <summary>
	///     Apply an amount of friction to the current velocity.
	///     No need to scale by time delta - it will be done inside.
	/// </summary>
	public void ApplyFriction( float frictionAmount, float stopSpeed = 140.0f )
	{
		var speed = Velocity.Length;
		if ( speed < 0.01f ) return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		var control = speed < stopSpeed ? stopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		var newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return;

		newspeed /= speed;
		Velocity *= newspeed;
	}

	private SceneTrace BuildTrace( Vector3 from, Vector3 to )
	{
		return BuildTrace( Scene.Trace.Ray( from, to ) );
	}

	private SceneTrace BuildTrace( SceneTrace source )
	{
		return source.Size( BoundingBox ).WithoutTags( IgnoreLayers ).IgnoreGameObjectHierarchy( GameObject );
	}

	private void Move( bool step )
	{
		if ( step && IsOnGround )
		{
			Velocity = Velocity.WithZ( 0 );
		}

		if ( Velocity.Length < 0.001f )
		{
			Velocity = Vector3.Zero;
			return;
		}

		var pos = GameObject.Transform.Position;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, Velocity );
		mover.Bounce = 0.3f;
		mover.MaxStandableAngle = GroundAngle;

		if ( step && IsOnGround )
		{
			mover.TryMoveWithStep( Time.Delta, StepHeight );
		}
		else
		{
			mover.TryMove( Time.Delta );
		}

		Transform.Position = mover.Position.WithY( 512f );

		LastVelocity = Velocity;
		Velocity = mover.Velocity;
	}

	public void ReleaseFromGround()
	{
		IsOnGround = false;
		Controller.LastGroundHeight = Transform.Position.z;
	}

	private void CategorizePosition()
	{
		var pos = Transform.Position;
		var point = pos + Vector3.Down * 2;
		var vBumpOrigin = pos;
		var wasOnGround = IsOnGround;

		// We're flying upwards too fast, never land on ground
		if ( !IsOnGround && Velocity.z > 50.0f )
		{
			IsOnGround = false;
			return;
		}

		//
		// trace down one step height if we're already on the ground "step down". If not, search for floor right below us
		// because if we do StepHeight we'll snap that many units to the ground
		//
		point.z -= wasOnGround ? StepHeight : 0.1f;

		var pm = BuildTrace( vBumpOrigin, point ).Run();

		var trForward = Scene.Trace.Ray( Transform.Position, Transform.Position + Transform.Rotation.Forward * 10f )
			.IgnoreGameObjectHierarchy( GameObject ).Run();
		var trBackward = Scene.Trace.Ray( Transform.Position, Transform.Position + Transform.Rotation.Backward * 10f )
			.IgnoreGameObjectHierarchy( GameObject ).Run();

		var squished = trForward.Hit && trBackward.Hit;
		var shouldGround = Vector3.GetAngle( Vector3.Up, pm.Normal ) <= GroundAngle || squished;

		//
		// we didn't hit - or the ground is too steep to be ground
		//
		if ( !pm.Hit || !shouldGround )
		{
			IsOnGround = false;
			return;
		}

		if ( !IsOnGround )
		{
			OnLanded();
		}

		//
		// we are on ground
		//
		IsOnGround = true;
		CurrentGroundAngle = Vector3.GetAngle( Transform.Rotation.Up, pm.Normal );
		if ( pm.Normal.x < 0 )
			CurrentGroundAngle *= -1;

		//
		// move to this ground position, if we moved, and hit
		//
		if ( wasOnGround && !pm.StartedSolid && pm.Fraction > 0.002f && pm.Fraction < 1.0f )
		{
			Transform.Position = pm.EndPosition + pm.Normal * 0.01f;
		}
	}

	protected void OnLanded()
	{
		Controller.CheckFallDamage();
		Velocity /= 1.8f;
		OnLandedEffects( Transform.Position );

		if ( Controller.IsHardFalling )
			Controller.Grub.OnHardFall();
	}

	[Broadcast]
	private void OnLandedEffects( Vector3 position )
	{
		var fallVelocity =
			Math.Clamp( Controller.FallVelocity, 0,
				1200 ); // Player won't reasonably be falling faster than 1200 so make this the upper limit

		var t = fallVelocity / 1200f;
		var radius = MathX.Lerp( 0.1f, 2f, t );

		var particles =
			ParticleHelper.Instance.PlayInstantaneous( LandingParticles, new Transform( position ) );
		particles.SetControlPoint( 1, new Vector3( radius, 0, 0 ) );
	}

	/// <summary>
	/// Disconnect from ground and punch our velocity. This is useful if you want the player to jump or something.
	/// </summary>
	public void Punch( in Vector3 amount )
	{
		IsOnGround = false;
		Velocity += amount;
	}

	/// <summary>
	/// Move a character, with this velocity
	/// </summary>
	public void Move()
	{
		if ( TryUnstuck() )
			return;

		if ( IsOnGround )
		{
			Move( true );
		}
		else
		{
			Move( false );
		}

		CategorizePosition();
	}

	/// <summary>
	/// Move from our current position to this target position, but using tracing an sliding.
	/// This is good for different control modes like ladders and stuff.
	/// </summary>
	public void MoveTo( Vector3 targetPosition, bool useStep )
	{
		if ( TryUnstuck() )
			return;

		var pos = Transform.Position;
		var delta = targetPosition - pos;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, delta );
		mover.MaxStandableAngle = GroundAngle;

		if ( useStep )
		{
			mover.TryMoveWithStep( 1.0f, StepHeight );
		}
		else
		{
			mover.TryMove( 1.0f );
		}

		Transform.Position = mover.Position;
	}

	private bool TryUnstuck()
	{
		var result = BuildTrace( Transform.Position, Transform.Position ).Run();

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			return false;
		}

		//using ( Gizmo.Scope( "unstuck", Transform.World ) )
		//{
		//	Gizmo.Draw.Color = Gizmo.Colors.Red;
		//	Gizmo.Draw.LineBBox( BoundingBox );
		//}

		var AttemptsPerTick = 20;

		for ( var i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Transform.Position + Vector3.Random.Normal * (_stuckTries / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = Transform.Position + Vector3.Up * 2;
			}

			result = BuildTrace( pos, pos ).Run();

			if ( !result.StartedSolid )
			{
				//Log.Info( $"unstuck after {_stuckTries} tries ({_stuckTries * AttemptsPerTick} tests)" );
				Transform.Position = pos;
				return false;
			}
		}

		_stuckTries++;

		return true;
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( IsOnGround );
		stream.Write( Velocity );
	}

	public void Read( ByteStream stream )
	{
		IsOnGround = stream.Read<bool>();
		Velocity = stream.Read<Vector3>();
	}
}
