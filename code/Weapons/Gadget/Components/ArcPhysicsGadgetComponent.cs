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
	protected IList<ArcSegment> Segments { get; set; }

	protected ExplosiveGadgetComponent ExplosiveComponent;
	float _alpha = 0;

	/// <summary>
	/// Debug console variable to see the projectiles path.
	/// </summary>
	[ConVar.Replicated( "projectile_debug" )]
	public static bool ProjectileDebug { get; set; }

	public override void OnUse( Weapon weapon, int charge )
	{
		base.OnUse( weapon, charge );
		Start( weapon.GetStartPosition(), Grub.EyeRotation.Forward.Normal * Grub.Facing, charge );
	}

	public void Start( Vector3 position, Vector3 dir, int charge )
	{
		Gadget.Position = position;
		Segments = CalculateTrajectory( dir, charge );
		Gadget.Position = Segments[0].StartPos;

		ExplosiveComponent = Gadget.Components.Get<ExplosiveGadgetComponent>();
	}

	public override bool IsResolved()
	{
		return false;
	}

	public override void Simulate( IClient client )
	{
		RunAlongSegments();
	}

	protected void RunAlongSegments()
	{
		if ( ProjectileDebug )
			DrawSegments();

		_alpha += Time.Delta * ProjectileSpeed;

		if ( Segments.Any() )
		{
			var currentSegment = Segments.FirstOrDefault();

			if ( _alpha >= 1f )
			{
				if ( Segments.Count == 1 )
				{
					UpdateGadget( currentSegment, 1f );
					Segments.RemoveAt( 0 );
					return;
				}

				_alpha = 0;
				Segments.RemoveAt( 0 );
			}

			UpdateGadget( currentSegment, _alpha );
		}
		else if ( ExplosiveComponent?.ExplodeAfter > 0 )
		{
			ExplosiveComponent?.ExplodeAfterSeconds( ExplosiveComponent.ExplodeAfter );
		}
		else
		{
			ExplosiveComponent?.Explode();
		}
	}

	void UpdateGadget( ArcSegment segment, float alpha )
	{
		// Hack: Why is this running before the gadget authority is lined up?
		if ( !Gadget.IsAuthority )
			return;

		Gadget.Rotation = Rotation.LookAt( segment.EndPos - segment.StartPos );
		Gadget.Velocity = (segment.EndPos - Gadget.Position) * ProjectileSpeed;
		Gadget.Position = Vector3.Lerp( segment.StartPos, segment.EndPos, alpha );
	}

	protected List<ArcSegment> CalculateTrajectory( Vector3 direction, int charge )
	{
		var force = charge * 0.5f;
		var arcTrace = new ArcTrace( Grub, Gadget, Gadget.Position );
		return ShouldBounce
			? arcTrace.RunTowardsWithBounces( direction, force, GamemodeSystem.Instance.ActiveWindForce, MaxBounces )
			: arcTrace.RunTowards( direction, force, GamemodeSystem.Instance.ActiveWindForce );
	}

	void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos, Game.IsServer ? Color.Red : Color.Green, 12f );

		DebugOverlay.Sphere( Segments.LastOrDefault().EndPos, 16f, Color.Blue, 12f );
	}
}
