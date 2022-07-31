namespace Grubs.Player;

public partial class WormController : BasePlayerController
{
	[Net] public float DefaultSpeed { get; set; } = 80f;
	[Net] public float Gravity { get; set; } = 800f;
	[Net] public float MaxStandableAngle { get; set; } = 60f;
	[Net] public bool IsGrounded { get; private set; } = false;
	[Net] public float Radius { get; set; } = 16f;

	protected Vector3 mins, maxs;
	private Vector3 lookPosition;

	public override void Simulate()
	{
		UpdateBBox();

		GetInput();

		if ( IsGrounded )
			Move();
		else
			AirMove();

		CheckGround();
		SetWormModelDirection();
	}

	private void GetInput()
	{
		var worm = Pawn as Worm;

		if ( worm.IsTurn )
		{
			WishVelocity = -Input.Left * Vector3.Forward;
			var speed = WishVelocity.Length.Clamp( 0, 1 );

			WishVelocity = WishVelocity.WithZ( 0f );
			WishVelocity = WishVelocity.Normal * speed;
			WishVelocity *= DefaultSpeed;

			Velocity = WishVelocity;
		}
	}

	public override BBox GetHull()
	{
		var girth = 24 * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, 32 );

		return new BBox( mins, maxs );
	}

	public void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	public void UpdateBBox()
	{
		var girth = 24 * 0.5f;

		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, 32 );

		SetBBox( mins, maxs );
	}

	private void Move()
	{
		var move = new MoveHelper( Position, Velocity )
		{
			MaxStandableAngle = MaxStandableAngle
		};

		move.Trace = move.Trace
			.Size( mins, maxs )
			.Ignore( Pawn );
		move.TryUnstuck();
		move.TryMoveWithStep( Time.Delta, 32f );

		Position = move.Position.WithY( 0f );
		Velocity = move.Velocity;
	}

	// TODO: Ensure worms can climb up slopes.
	private void AirMove()
	{
		var wishDir = WishVelocity.Normal;
		var wishSpeed = WishVelocity.Length;

		Velocity += wishDir * wishSpeed;
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f );

		Move();
	}

	private void CheckGround()
	{
		var tr = Trace.Ray( Position, Position + (Vector3.Down * 0.2f) )
			.WorldOnly()
			.Run();

		IsGrounded = tr.Hit;
	}

	private void SetWormModelDirection()
	{
		lookPosition = Velocity.Normal.WithZ( 0 ).IsNearZeroLength ? lookPosition : Velocity.WithZ( 0 ).Normal;
		EyeRotation = Rotation.LookAt( lookPosition );

		if ( Velocity.Normal.IsNearZeroLength )
			return;

		float direction = Pawn.EyeRotation.Forward.Dot( Pawn.Rotation.Forward );

		if ( direction < 0 )
		{
			Rotation *= Rotation.From( 0, 180, 0 );
			Pawn.ResetInterpolation();
		}
	}
}
