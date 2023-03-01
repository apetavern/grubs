namespace Grubs;

[Prefab]
public partial class ProjectileComponent : ExplosiveComponent
{
	[Prefab]
	public bool ProjectileShouldUseTrace { get; set; } = false;

	[Prefab]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	private List<ArcSegment> Segments { get; set; } = new();

	public override void OnStart()
	{
		// TODO: What about bouncing?
		var arcTrace = new ArcTrace( Grub, Grub.Player.MousePosition.WithZ( 1000 ) );
		Segments = arcTrace.RunTowards( -Vector3.Up, Explosive.ExplosionForceMultiplier * 10f, 0f ); // With charge?
		Explosive.Position = Segments[0].StartPos;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( Segments.Count > 0 )
		{
			HandleSegmentTick();
		}
	}

	private void HandleSegmentTick()
	{
		if ( (Segments[0].EndPos - Explosive.Position).IsNearlyZero( 2.5f ) )
		{
			if ( Segments.Count > 1 )
				Segments.RemoveAt( 0 );
			// else
			// 	OnCollision();

			return;
		}

		Explosive.Rotation = Rotation.LookAt( Segments[0].EndPos - Segments[0].StartPos );
		Explosive.Position = Vector3.Lerp( Segments[0].StartPos, Segments[0].EndPos, Time.Delta / (1 / ProjectileSpeed) );
	}
}
