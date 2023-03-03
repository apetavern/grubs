namespace Grubs;

[Prefab]
public partial class ProjectileExplosiveComponent : ExplosiveComponent
{
	[Prefab, Net]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	[Prefab, Net]
	public bool ShouldBounce { get; set; } = false;

	[Prefab, Net]
	public int MaxBounces { get; set; } = 0;

	[Net]
	private IList<ArcSegment> Segments { get; set; }

	/// <summary>
	/// Debug console variable to see the projectiles path.
	/// </summary>
	[ConVar.Replicated( "projectile_debug" )]
	public static bool ProjectileDebug { get; set; }

	private void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos, Game.IsServer ? Color.Red : Color.Green );
	}

	public override void OnFired( Weapon weapon, int charge )
	{
		base.OnFired( weapon, charge );

		var position = weapon.Position.WithY( 0f );
		var muzzle = weapon.GetAttachment( "muzzle" );
		if ( muzzle is not null )
			position = muzzle.Value.Position.WithY( 0f );

		Explosive.Position = position;

		if ( Explosive.UseCustomPhysics )
		{
			var arcTrace = new ArcTrace( Grub, Grub.EyePosition );
			Segments = ShouldBounce
				? arcTrace.RunTowardsWithBounces( Grub.EyeRotation.Forward.Normal * Grub.Facing, Explosive.ExplosionForceMultiplier * charge, 0, MaxBounces )
				: arcTrace.RunTowards( Grub.EyeRotation.Forward.Normal * Grub.Facing, Explosive.ExplosionForceMultiplier * charge, 0f );
			Explosive.Position = Segments[0].StartPos;
		}
		else
		{
			var desiredPosition = position + (Grub.EyeRotation.Forward.Normal * Grub.Facing * 40f);
			var tr = Trace.Ray( desiredPosition, desiredPosition ).Ignore( Grub ).Run(); // This trace is incorrect, should be from position -> desired position.
			Explosive.Position = tr.EndPosition;
			Explosive.Velocity = (Grub.EyeRotation.Forward.Normal * Grub.Facing * charge * ProjectileSpeed).WithY( 0f );
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !Explosive.UseCustomPhysics )
			return;

		if ( ProjectileDebug )
			DrawSegments();

		var currentSegment = Segments.First();

		Explosive.Rotation = Rotation.LookAt( currentSegment.EndPos - currentSegment.StartPos );
		Explosive.Position = Vector3.Lerp( currentSegment.StartPos, currentSegment.EndPos, Time.Delta / (1 / ProjectileSpeed) );

		if ( (currentSegment.EndPos - Explosive.Position).IsNearlyZero( 2.5f ) )
		{
			if ( Segments.Count > 1 )
				Segments.RemoveAt( 0 );
			else
				ExplodeAfterSeconds( Explosive.ExplodeAfter );

			return;
		}
	}
}
