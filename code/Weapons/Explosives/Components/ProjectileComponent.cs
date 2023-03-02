namespace Grubs;

[Prefab]
public partial class ProjectileComponent : ExplosiveComponent
{
	[Prefab]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	private List<ArcSegment> Segments { get; set; } = new();

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
		if ( Explosive.UseCustomPhysics )
		{
			var arcTrace = new ArcTrace( Grub, Grub.EyePosition );
			Segments = arcTrace.RunTowards( Grub.EyeRotation.Forward.Normal * Grub.Facing, Explosive.ExplosionForceMultiplier * charge, 0f );
			Explosive.Position = Segments[0].StartPos;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( Explosive.UseCustomPhysics && ProjectileDebug )
			DrawSegments();

		if ( (Segments[0].EndPos - Explosive.Position).IsNearlyZero( 2.5f ) )
		{
			if ( Segments.Count > 1 )
				Segments.RemoveAt( 0 );
			else
				Explode();

			return;
		}

		Explosive.Rotation = Rotation.LookAt( Segments[0].EndPos - Segments[0].StartPos );
		Explosive.Position = Vector3.Lerp( Segments[0].StartPos, Segments[0].EndPos, Time.Delta / (1 / ProjectileSpeed) );
	}
}
