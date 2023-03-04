namespace Grubs;

[Prefab]
public partial class ArcPhysicsGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	[Prefab, Net]
	public bool ShouldBounce { get; set; } = false;

	[Prefab, Net]
	public int MaxBounces { get; set; } = 0;

	[Net]
	private IList<ArcSegment> Segments { get; set; }

	private ExplosiveGadgetComponent _explosiveComponent;

	/// <summary>
	/// Debug console variable to see the projectiles path.
	/// </summary>
	[ConVar.Replicated( "projectile_debug" )]
	public static bool ProjectileDebug { get; set; }

	public override void OnUse( Weapon weapon, int charge )
	{
		base.OnUse( weapon, charge );

		_explosiveComponent = Gadget.Components.Get<ExplosiveGadgetComponent>();
		var forceMultiplayer = _explosiveComponent?.ExplosionForceMultiplier ?? 1;

		var arcTrace = new ArcTrace( Grub, Grub.EyePosition );
		Segments = ShouldBounce
			? arcTrace.RunTowardsWithBounces( Grub.EyeRotation.Forward.Normal * Grub.Facing, forceMultiplayer * charge, 0, MaxBounces )
			: arcTrace.RunTowards( Grub.EyeRotation.Forward.Normal * Grub.Facing, forceMultiplayer * charge, 0f );
		Gadget.Position = Segments[0].StartPos;
	}

	public override void Simulate( IClient client )
	{
		if ( ProjectileDebug )
			DrawSegments();

		if ( Segments.Any() )
		{
			var currentSegment = Segments.FirstOrDefault();

			Gadget.Rotation = Rotation.LookAt( currentSegment.EndPos - currentSegment.StartPos );
			Gadget.Position = Vector3.Lerp( currentSegment.StartPos, currentSegment.EndPos, Time.Delta / (1 / ProjectileSpeed) );

			if ( (currentSegment.EndPos - Gadget.Position).IsNearlyZero( 2.5f ) )
				Segments.RemoveAt( 0 );
		}
		else
		{
			_explosiveComponent?.ExplodeAfterSeconds( _explosiveComponent.ExplodeAfter );
		}
	}

	private void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos, Game.IsServer ? Color.Red : Color.Green );
	}
}
