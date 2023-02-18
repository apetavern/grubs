namespace Grubs;

public partial class GrubController : EntityComponent<Grub>
{
	public Grub Grub => Entity;

	public Vector3 Position
	{
		get => Grub.Position;
		set => Grub.Position = value;
	}

	public Vector3 Velocity
	{
		get => Grub.Velocity;
		set => Grub.Velocity = value;
	}

	public Vector3 BaseVelocity { get; set; }
	public Vector3 LastVelocity { get; set; }
	public Vector3 WishVelocity { get; set; }

	public Entity GroundEntity { get; set; }
	public Entity LastGroundEntity { get; set; }
	public Vector3 GroundNormal { get; set; }
	public float CurrentGroundAngle { get; set; }

	public bool IsGrounded
	{
		get => GroundEntity != null;
	}

	public static float BodyGirth => 20f;
	public static float EyeHeight => 28f;

	[Net, Predicted]
	public float CurrentEyeHeight { get; set; } = 28f;

	public IEnumerable<ControllerMechanic> Mechanics => Entity.Components.GetAll<ControllerMechanic>();
	public ControllerMechanic BestMechanic => Mechanics.OrderByDescending( x => x.SortOrder ).FirstOrDefault( x => x.IsActive );

	public T GetMechanic<T>() where T : ControllerMechanic
	{
		foreach ( var mechanic in Mechanics )
		{
			if ( mechanic is T val )
				return val;
		}

		return null;
	}

	public bool IsMechanicActive<T>() where T : ControllerMechanic
	{
		return GetMechanic<T>()?.IsActive ?? false;
	}

	public BBox Hull
	{
		get
		{
			var girth = BodyGirth * 0.5f;

			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, CurrentEyeHeight );

			return new BBox( mins, maxs );
		}
	}

	public virtual void Simulate( IClient client )
	{
		SimulateEyes();
		SimulateMechanics();
		UpdateRotation();

		if ( Debug && Grub.IsTurn )
		{
			var hull = Hull;
			DebugOverlay.Box( Position, hull.Mins, hull.Maxs, Color.Red );
			DebugOverlay.Box( Position, hull.Mins, hull.Maxs, Color.Blue );

			var lineOffset = 0;

			DebugOverlay.ScreenText( $"Player Controller", ++lineOffset );
			DebugOverlay.ScreenText( $"        Position: {Position}", ++lineOffset );
			DebugOverlay.ScreenText( $"        Velocity: {Velocity}", ++lineOffset );
			DebugOverlay.ScreenText( $"    BaseVelocity: {BaseVelocity}", ++lineOffset );
			DebugOverlay.ScreenText( $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]", ++lineOffset );
			DebugOverlay.ScreenText( $"           Speed: {Velocity.Length}", ++lineOffset );

			++lineOffset;
			DebugOverlay.ScreenText( $"Mechanics", ++lineOffset );
			foreach ( var mechanic in Mechanics )
			{
				DebugOverlay.ScreenText( $"{mechanic}", ++lineOffset );
			}
		}
	}

	public virtual void FrameSimulate( IClient client )
	{
		SimulateEyes();
	}

	protected void SimulateEyes()
	{
		Grub.Facing = Grub.Rotation.z < 0 ? -1 : 1;
		Grub.EyeRotation = Grub.LookAngles.ToRotation();
		Grub.EyeLocalPosition = Vector3.Up * CurrentEyeHeight;
	}

	protected void SimulateMechanics()
	{
		foreach ( var mechanic in Mechanics )
		{
			mechanic.TrySimulate( this );
		}
	}

	protected void UpdateRotation()
	{
		if ( Velocity.Normal.IsNearZeroLength || (Grub.IsTurn && !Grub.Controller.IsGrounded) )
			return;

		var facing = Velocity.Normal.x;
		if ( facing.AlmostEqual( 1, within: 0.5f ) )
		{
			Grub.Rotation = Rotation.Identity;
		}
		else if ( facing.AlmostEqual( -1, within: 0.5f ) )
		{
			Grub.Rotation = Rotation.From( 0, 180, 0 );
		}
	}

	public virtual TraceResult TraceBBox(
		Vector3 start,
		Vector3 end,
		Vector3 mins,
		Vector3 maxs,
		float liftFeet = 0.0f,
		float liftHead = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		var tr = Trace.Ray( start, end )
			.Size( mins, maxs )
			.WithAnyTags( "solid", "player" )
			.Ignore( Grub )
			.Run();

		return tr;
	}

	public virtual TraceResult TraceBBox(
		Vector3 start,
		Vector3 end,
		float liftFeet = 0.0f,
		float liftHead = 0.0f )
	{
		var hull = Hull;
		return TraceBBox( start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
	}

	// A Grub can only move left and right for now.
	public float GetWishInput()
	{
		return ShouldAllowMovement() ? Grub.MoveInput : 0f;
	}

	public bool ShouldAllowMovement()
	{
		var gm = GamemodeSystem.Instance;
		return Grub.IsTurn && !gm.TurnIsChanging && gm.AllowMovement;
	}

	public bool ShouldShowWeapon()
	{
		return Grub.IsTurn && Velocity.IsNearlyZero( 2.5f ) && IsGrounded;
	}

	public Vector3 GetWishVelocity( bool zeroPitch = false )
	{
		var result = new Vector3().WithX( -GetWishInput() );
		var inSpeed = result.Length.Clamp( 0, 1 );

		if ( zeroPitch )
			result.z = 0;

		result = result.Normal * inSpeed;
		result *= GetWishSpeed();

		return result;
	}

	public virtual float GetWishSpeed()
	{
		return BestMechanic?.WishSpeed ?? 80f;
	}

	public void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = Velocity.Dot( wishdir );
		var addspeed = wishspeed - currentspeed;

		if ( addspeed <= 0 )
			return;

		var accelspeed = acceleration * Time.Delta * wishspeed;

		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
		Velocity = Velocity.WithY( 0 );
	}

	public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
	{
		var speed = Velocity.Length;
		if ( speed.AlmostEqual( 0f ) ) return;

		var control = (speed < stopSpeed) ? stopSpeed : speed;
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

	public void StepMove( float groundAngle = 46f, float stepSize = 18f )
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Hull )
			.Ignore( Grub );
		mover.MaxStandableAngle = groundAngle;

		mover.TryMoveWithStep( Time.Delta, stepSize );

		Position = mover.Position.WithY( 0f );
		Velocity = mover.Velocity;
	}

	public void Move( float groundAngle = 46f )
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Hull )
			.Ignore( Grub );
		mover.MaxStandableAngle = groundAngle;

		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	[ConVar.Replicated( "gr_debug_playercontroller" )]
	public static bool Debug { get; set; } = false;
}
