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

	protected ExplosiveGadgetComponent _explosiveComponent;
	private float _alpha = 0;

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

		_explosiveComponent = Gadget.Components.Get<ExplosiveGadgetComponent>();
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

		if ( Segments.Any() )
		{
			var currentSegment = Segments.FirstOrDefault();

			_alpha += Time.Delta * ProjectileSpeed;
			if ( _alpha >= 1f )
			{
				_alpha = 0;
				Segments.RemoveAt( 0 );
			}

			Gadget.Rotation = Rotation.LookAt( currentSegment.EndPos - currentSegment.StartPos );
			Gadget.Velocity = (currentSegment.EndPos - Gadget.Position) * ProjectileSpeed;
			Gadget.Position = Vector3.Lerp( currentSegment.StartPos, currentSegment.EndPos, _alpha );
		}
		else if ( _explosiveComponent?.ExplodeAfter > 0 )
		{
			_explosiveComponent?.ExplodeAfterSeconds( _explosiveComponent.ExplodeAfter );
		}
		else
		{
			_explosiveComponent?.Explode();
		}
	}

	public List<ArcSegment> CalculateTrajectory( Vector3 direction, int charge )
	{
		var force = charge * 0.5f;
		var arcTrace = new ArcTrace( Grub, Gadget, Gadget.Position );
		return ShouldBounce
			? arcTrace.RunTowardsWithBounces( direction, force, GamemodeSystem.Instance.ActiveWindForce, MaxBounces )
			: arcTrace.RunTowards( direction, force, GamemodeSystem.Instance.ActiveWindForce );
	}

	private void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos, Game.IsServer ? Color.Red : Color.Green );
	}
}
