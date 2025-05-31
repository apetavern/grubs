using Grubs.Helpers;
using Grubs.Terrain;
using Sandbox.Utility;

namespace Grubs.Systems.Pawn.Grubs.Controller;

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

	public BBox BoundingBox => new( new Vector3( -Radius, -Radius, 0 ), new Vector3( Radius, Radius, Height ) );

	[Sync] public Vector3 Velocity { get; private set; }

	[Sync] public bool IsOnGround { get; set; }

	[Sync] public float CurrentGroundAngle { get; set; }

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineBBox( BoundingBox );
	}

	public void SetVelocity( Vector3 velocity )
	{
		if ( IsProxy )
			Log.Error( $"Setting velocity from non-owner?! WTF?!" );
		Velocity = velocity;
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

		// Reduce wish speed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = Acceleration * Time.Delta * wishspeed;

		// Cap at add speed
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
		if ( speed < 0.01f ) 
			return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		var control = speed < stopSpeed ? stopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		var newSpeed = speed - drop;
		if ( newSpeed < 0 ) newSpeed = 0;
		if ( newSpeed == speed ) 
			return;

		newSpeed /= speed;
		Velocity *= newSpeed;
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

		var pos = GameObject.WorldPosition;

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

		WorldPosition = mover.Position.WithY( 512f );

		LastVelocity = Velocity;
		Velocity = mover.Velocity;
	}

	public void ReleaseFromGround()
	{
		IsOnGround = false;
		Controller.LastGroundHeight = WorldPosition.z;
		CurrentGroundAngle = 0;
	}

	private void CategorizePosition()
	{
		var pos = WorldPosition;
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

		var trForward = Scene.Trace.Ray( WorldPosition, WorldPosition + WorldRotation.Forward * 10f )
			.IgnoreGameObjectHierarchy( GameObject ).Run();
		var trBackward = Scene.Trace.Ray( WorldPosition, WorldPosition + WorldRotation.Backward * 10f )
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
		CurrentGroundAngle = Vector3.GetAngle( WorldRotation.Up, pm.Normal );
		if ( pm.Normal.x < 0 )
			CurrentGroundAngle *= -1;

		//
		// move to this ground position, if we moved, and hit
		//
		if ( wasOnGround && !pm.StartedSolid && pm.Fraction > 0.002f && pm.Fraction < 1.0f )
		{
			WorldPosition = pm.EndPosition + pm.Normal * 0.01f;
		}
	}

	protected void OnLanded()
	{
		Controller.CheckFallDamage();
		Velocity /= 1.8f;
		OnLandedEffects( WorldPosition );

		if ( Controller.IsHardFalling )
			Controller.Grub.OnHardFall();
	}

	private const string LandingParticlesPath = "particles/landimpact/landing_impact.prefab";

	[Rpc.Broadcast]
	private void OnLandedEffects( Vector3 position )
	{
		var fallVelocity =
			Math.Clamp( Controller.FallVelocity, 0,
				1200 ); // Player won't reasonably be falling faster than 1200 so make this the upper limit

		var t = fallVelocity / 1200f;
		var radius = MathX.Lerp( 0.1f, 2f, t );
		
		var landingParticles = GameObject.Clone( LandingParticlesPath );
		landingParticles.WorldPosition = position;
		
		Log.Info( radius );

		var emitter = landingParticles.GetComponent<ParticleRingEmitter>();
		emitter.Radius = radius / 10f;
		
		var particleRenderer = landingParticles.GetComponent<ParticleModelRenderer>();
		particleRenderer.Scale = (radius * 0.35f).Clamp( 0.25f, 0.35f );
	}

	/// <summary>
	/// Disconnect from ground and punch our velocity. This is useful if you want the player to jump or something.
	/// </summary>
	public void Punch( in Vector3 amount )
	{
		ReleaseFromGround();

		Velocity += amount;
		if ( Input.UsingController ) Input.TriggerHaptics( amount.Length / 1000f, amount.Length / 1000f );
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

		var pos = WorldPosition;
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

		WorldPosition = mover.Position;
	}

	private bool TryUnstuck()
	{
		// Check for being stuck inside a non-terrain object (like a player)
		var result = BuildTrace( WorldPosition, WorldPosition ).Run();
		var terrainCheck = Scene.Trace.Ray( WorldPosition, WorldPosition + Vector3.Right * 64f )
			.Size( BoundingBox )
			.WithTag( "solid" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();
		
		// Stuck inside of something
		if ( result.StartedSolid && result.GameObject.IsValid() )
		{
			// Don't let active grub push this grub around
			var activePlayer = Player.All.FirstOrDefault( p => p.IsActive );
			if ( activePlayer.IsValid() )
			{
				if ( result.GameObject.Tags.Has( "player" ) && result.GameObject.Parent == activePlayer.ActiveGrub?.GameObject )
				{
					_stuckTries = 0;
					return false;
				}
			}
		} 
		// TODO: Work this into the trace if it ever starts working
		else if ( !terrainCheck.Hit || !terrainCheck.GameObject.Tags.Contains( "terrain" ) )
		{
			_stuckTries = 0;
			return false;
		}

		var attemptsPerTick = 20;

		for ( var i = 0; i < attemptsPerTick; i++ )
		{
			var pos = WorldPosition + Vector3.Random.Normal * (_stuckTries / 2.0f);
			pos = pos.WithY( 512f );

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = WorldPosition + Vector3.Up * 2;
			}

			result = BuildTrace( pos, pos ).Run();
			
			// Don't let them spawn inside the terrain
			if ( GrubsTerrain.Instance.PointInside( pos ) )
				continue;

			if ( !result.StartedSolid )
			{
				WorldPosition = pos;
				return false;
			}
		}

		_stuckTries++;

		return true;
	}
}
