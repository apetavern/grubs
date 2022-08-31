using Grubs.States;
using Grubs.Utils.Extensions;

namespace Grubs.Player;

public partial class GrubController : BasePlayerController
{
	[Net] public float DefaultSpeed { get; set; } = 80.0f;
	[Net] public float Acceleration { get; set; } = 10.0f;
	[Net] public float AirAcceleration { get; set; } = 80.0f;
	[Net] public float GroundFriction { get; set; } = 4.0f;
	[Net] public float StopSpeed { get; set; } = 100.0f;
	[Net] public float GroundAngle { get; set; } = 46.0f;
	[Net] public float StepSize { get; set; } = 8.0f;
	[Net] public float MaxNonJumpVelocity { get; set; } = 140.0f;
	[Net] public float Gravity { get; set; } = 800.0f;
	[Net] public float AirControl { get; set; } = 120.0f;
	public bool IsGrounded { get; private set; }
	public float FallVelocity { get; private set; }

	// Aim properties
	private Vector3 LookPos { get; set; }
	private float LookRotOffset { get; set; }

	public Unstuck Unstuck;
	protected float SurfaceFriction;
	protected Vector3 mins;
	protected Vector3 maxs;

	public GrubController()
	{
		Unstuck = new Unstuck( this );
		Debug = false;
	}

	public override BBox GetHull()
	{
		var girth = 32 * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, 32 );

		return new BBox( mins, maxs );
	}

	public virtual void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	public virtual void UpdateBBox()
	{
		var girth = 32 * 0.5f;

		var mins = new Vector3( -girth, -girth, 2 ) * Pawn.Scale;
		var maxs = new Vector3( +girth, +girth, 32 ) * Pawn.Scale;

		SetBBox( mins, maxs );
	}

	public override void Simulate()
	{
		var grub = Pawn as Grub;
		FallVelocity = -grub!.Velocity.z;

		var isFiring = false;
		if ( grub.ActiveChild is not null && grub.ActiveChild.IsValid() )
			isFiring = grub.ActiveChild.IsFiring;

		UpdateBBox();
		SetEyeTransform( isFiring );

		if ( Unstuck.TestAndFix() )
			return;

		// Start Gravity
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

		BaseVelocity = BaseVelocity.WithZ( 0 );

		if ( Input.Pressed( InputButton.Jump ) && grub.IsTurn && !isFiring )
		{
			CheckJumpButton();
		}

		// Friction is handled before we add in any base velocity. That way, if we are on a conveyor,
		// we don't slow when standing still, relative to the conveyor.
		if ( IsGrounded )
		{
			Velocity = Velocity.WithZ( 0 );

			if ( IsGrounded )
			{
				ApplyFriction( GroundFriction * SurfaceFriction );
			}
		}

		// Take Input if it is currently the grubs turn. Don't allow movement while jumping.
		WishVelocity = grub.IsTurn && !isFiring && IsGrounded && !GrubsGame.Current.CurrentGamemode.UsedTurn
			? -Input.Left * Vector3.Forward
			: Vector3.Zero;
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= DefaultSpeed;

		bool bStayOnGround = false;
		if ( IsGrounded )
		{
			bStayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( bStayOnGround );

		// FinishGravity
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		if ( IsGrounded )
		{
			Velocity = Velocity.WithZ( 0 );
		}

		CheckFalling();

		grub.ActiveChild?.ShowWeapon( grub, Velocity.IsNearlyZero( 2.5f ) && IsGrounded );
	}

	public float FallPunchThreshold => 350f; // won't make player's screen / make scrape noise unless player falling at least this fast.
	public float PlayerLandOnFloatingObject => 173; // Can fall another 173 in/sec without getting hurt
	public float PlayerMaxSafeFallSpeed => MathF.Sqrt( 2 * Gravity * 20 * 12 ); // approx 20 feet
	public float PlayerFatalFallSpeed => MathF.Sqrt( 2 * Gravity * 60 * 12 ); // approx 60 feet
	public float DamageForFallSpeed => 100.0f / (PlayerFatalFallSpeed - PlayerMaxSafeFallSpeed);

	private void CheckFalling()
	{
		if ( GroundEntity == null || FallVelocity <= 0 )
		{
			return;
		}

		if ( Pawn.LifeState == LifeState.Dead || FallVelocity < FallPunchThreshold || Pawn.WaterLevel >= 1f )
		{
			return;
		}

		if ( GroundEntity.WaterLevel >= 1f )
		{
			FallVelocity -= PlayerLandOnFloatingObject;
		}

		if ( GroundEntity.Velocity.z < 0.0f )
		{
			FallVelocity += GroundEntity.Velocity.z;
			FallVelocity = MathF.Max( 0.1f, FallVelocity );
		}

		if ( FallVelocity > PlayerMaxSafeFallSpeed )
		{
			TakeFallDamage();
		}
	}

	private void TakeFallDamage()
	{
		if ( Host.IsServer && TeamManager.Instance.CurrentTeam.ActiveGrub == Pawn )
			GrubsGame.Current.CurrentGamemode.UseTurn();

		float fallDamage = (FallVelocity - PlayerMaxSafeFallSpeed) * DamageForFallSpeed;
		Pawn.TakeDamage( DamageInfoExtension.FromFall( fallDamage, Pawn ) );
	}

	private void SetEyeTransform( bool isFiring )
	{

		// Calculate eye position in world.
		EyeLocalPosition = new Vector3( 0, 0, 24 );

		// Set EyeRot to face the way we're walking.
		LookPos = Velocity.Normal.WithZ( 0 ).IsNearZeroLength ? LookPos : Velocity.WithZ( 0 ).Normal;

		// Only allow aiming changes if the grub isn't moving and not currently charging a weapon shot.
		if ( (Pawn as Grub)!.IsTurn && !isFiring )
		{
			// Aim with W & S keys
			EyeRotation = Rotation.LookAt( LookPos );

			if ( Velocity.IsNearlyZero( 1f ) )
				LookRotOffset = Math.Clamp( LookRotOffset + Input.Forward * 2, -45, 75 );

			// Rotate EyeRot by our offset
			EyeRotation = EyeRotation.RotateAroundAxis( EyeRotation.Left, LookRotOffset );
		}

		// Recalculate the grubs rotation if we're moving.
		if ( !Velocity.Normal.IsNearZeroLength )
			UpdateRotation();
	}

	private void UpdateRotation()
	{
		float grubFacing = Pawn.EyeRotation.Forward.Dot( Pawn.Rotation.Forward );

		if ( grubFacing < 0 )
		{
			Rotation *= Rotation.From( 0, 180, 0 );
			Pawn.ResetInterpolation();
		}
	}

	public virtual void WalkMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * wishspeed;

		Velocity = Velocity.WithZ( 0 );
		Accelerate( wishdir, wishspeed, 0, Acceleration );
		Velocity = Velocity.WithZ( 0 );

		// Add in any base velocity to the current velocity.
		Velocity += BaseVelocity;

		try
		{
			if ( Velocity.Length < 1.0f )
			{
				Velocity = Vector3.Zero;
				return;
			}

			var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

			var pm = TraceBBox( Position, dest );

			if ( pm.Fraction == 1 )
			{
				Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{
			// Now pull the base velocity back out.
			// Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
			Velocity -= BaseVelocity;
		}

		StayOnGround();
	}

	public virtual void StepMove()
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = GroundAngle;

		mover.TryMoveWithStep( Time.Delta, StepSize );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	public virtual void Move()
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = GroundAngle;

		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	/// <summary>
	/// Add our wish direction and speed onto our velocity
	/// </summary>
	public virtual void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		// See if we are changing direction a bit
		var currentspeed = Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
	}

	/// <summary>
	/// Remove ground friction from velocity
	/// </summary>
	public virtual void ApplyFriction( float frictionAmount = 1.0f )
	{
		// Calculate speed
		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount
		var drop = control * Time.Delta * frictionAmount;

		// Scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}

	public virtual void CheckJumpButton()
	{
		if ( !IsGrounded )
			return;

		ClearGroundEntity();

		float flGroundFactor = 1.0f;

		float flMul = 268.3281572999747f * 1.5f;

		float startz = Velocity.z;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );

		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		AddEvent( "jump" );
	}

	public virtual void AirMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

		Velocity += BaseVelocity;

		Move();

		Velocity -= BaseVelocity;
	}

	public virtual void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;

		bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
		bool bMoveToEndPos = false;

		if ( IsGrounded )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly )
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition.WithY( 0 );
		}

	}

	public virtual void UpdateGroundEntity( TraceResult tr )
	{
		GroundNormal = tr.Normal;

		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		GroundEntity = tr.Entity;
		if ( tr.Entity != null )
			IsGrounded = true;
	}

	public virtual void ClearGroundEntity()
	{
		if ( !IsGrounded ) return;

		IsGrounded = false;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1.0f;
	}

	/// <summary>
	/// Traces the current bbox and returns the result.
	/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		// TODO: If/when terrain is networked revert this
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
			.Size( mins, maxs )
			.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
			.Ignore( Pawn )
			.IncludeClientside()
			.Run();

		tr.EndPosition -= TraceOffset;
		return tr;

		// return TraceBBox( start, end, mins, maxs, liftFeet );
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc
	/// </summary>
	public virtual void StayOnGround()
	{
		var start = Position + Vector3.Up * 2;
		var end = Position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = TraceBBox( Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		Position = trace.EndPosition.WithY( 0 );
	}
}
