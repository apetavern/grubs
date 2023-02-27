namespace Grubs;

public partial class Projectile : Explosive
{
	private float Speed { get; set; } = 0.001f;
	private float CollisionExplosionDelaySeconds { get; set; }
	private List<ArcSegment> Segments { get; set; } = new();

	/// <summary>
	/// Sets the speed the projectile will move at.
	/// </summary>
	/// <param name="speed">The speed to move at.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithSpeed( float speed )
	{
		Speed = 1 / speed;
		return this;
	}

	/// <summary>
	/// Sets the delay to which the projectile will explode after colliding.
	/// </summary>
	/// <param name="delaySeconds">The seconds to delay for.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile WithCollisionExplosionDelay( float delaySeconds )
	{
		CollisionExplosionDelaySeconds = delaySeconds;
		return this;
	}

	/// <summary>
	/// Sets a path for the projectile to follow.
	/// </summary>
	/// <param name="points">The arc trace segments to follow.</param>
	/// <returns>The projectile instance.</returns>
	public Projectile MoveAlongTrace( List<ArcSegment> points )
	{
		Segments = points;

		// Set the initial position
		Position = Segments[0].StartPos;

		return this;
	}

	public override void Simulate( IClient client )
	{
		if ( Segments.Count == 0 )
		{
			base.Simulate( client );
			return;
		}

		if ( ProjectileDebug )
			DrawSegments();

		HandleSegmentTick();
	}

	private void HandleSegmentTick()
	{
		if ( (Segments[0].EndPos - Position).IsNearlyZero( 2.5f ) )
		{
			if ( Segments.Count > 1 )
				Segments.RemoveAt( 0 );
			else
				OnCollision();

			return;
		}

		Rotation = Rotation.LookAt( Segments[0].EndPos - Segments[0].StartPos );
		Position = Vector3.Lerp( Segments[0].StartPos, Segments[0].EndPos, Time.Delta / Speed );
	}

	private void OnCollision()
	{
		if ( !Game.IsServer )
			return;

		if ( CollisionExplosionDelaySeconds > 0 )
		{
			ExplodeAfterSeconds( CollisionExplosionDelaySeconds );
			return;
		}

		Explode();
	}

	/// <summary>
	/// Debug console variable to see the projectiles path.
	/// </summary>
	[ConVar.Replicated( "projectile_debug" )]
	public static bool ProjectileDebug { get; set; }

	private void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos );
	}
}
