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

	private float _alpha = 0;
	private ExplosiveGadgetComponent _explosiveComponent;

	[Net]
	bool DoHoming { get; set; } = false;

	[Net]
	Vector3 Target { get; set; } = Vector3.Zero;

	[Net]
	TimeSince TimeSinceFired { get; set; }

	/// <summary>
	/// Debug console variable to see the projectiles path.
	/// </summary>
	[ConVar.Replicated( "projectile_debug" )]
	public static bool ProjectileDebug { get; set; }

	public override void OnUse( Weapon weapon, int charge )
	{
		base.OnUse( weapon, charge );

		Target = weapon.Target;
		TimeSinceFired = 0f;

		_explosiveComponent = Gadget.Components.Get<ExplosiveGadgetComponent>();
		var forceMultiplayer = _explosiveComponent?.ExplosionForceMultiplier ?? 1;

		var arcTrace = new ArcTrace( Grub, Gadget, weapon.GetStartPosition() );
		Segments = ShouldBounce
			? arcTrace.RunTowardsWithBounces( Grub.EyeRotation.Forward.Normal * Grub.Facing, forceMultiplayer * charge, GamemodeSystem.Instance.ActiveWindForce, MaxBounces )
			: arcTrace.RunTowards( Grub.EyeRotation.Forward.Normal * Grub.Facing, forceMultiplayer * charge, GamemodeSystem.Instance.ActiveWindForce );
		Gadget.Position = Segments[0].StartPos;
	}

	public override bool IsResolved()
	{
		return false;
	}

	public override void Simulate( IClient client )
	{
		if ( ProjectileDebug )
			DrawSegments();

		if ( Target != Vector3.Zero && TimeSinceFired > 0.6f && TimeSinceFired < 5f && !DoHoming )
		{
			DoHoming = true;
			Gadget.PlayScreenSound( "beep" );
			Gadget.Velocity = Gadget.Rotation.Forward * ProjectileSpeed / 2f;
		}

		if ( Segments.Any() && !DoHoming )
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
		else if ( !DoHoming )
		{
			_explosiveComponent?.ExplodeAfterSeconds( _explosiveComponent.ExplodeAfter );
		}

		if ( DoHoming )
		{
			var rotation = Rotation.LookAt( (Target - Gadget.Position).WithY( 0 ), Vector3.Right );

			Gadget.Rotation = Rotation.Slerp( Gadget.Rotation, rotation, 0.075f );
			Gadget.Velocity = Vector3.Lerp( Gadget.Velocity, Gadget.Rotation.Forward * ProjectileSpeed / 4f, 0.5f );
			Gadget.Position += Gadget.Velocity;

			Gadget.Position = Gadget.Position.WithY( 0 );

			if ( TimeSinceFired > 5f )
			{
				DoHoming = false;
				var arcTrace = new ArcTrace( Grub, Gadget, Gadget.Position );
				Segments = ShouldBounce
					? arcTrace.RunTowardsWithBounces( Gadget.Velocity, ProjectileSpeed / 10f, GamemodeSystem.Instance.ActiveWindForce, MaxBounces )
					: arcTrace.RunTowards( Gadget.Velocity, ProjectileSpeed / 10f, GamemodeSystem.Instance.ActiveWindForce );
				return;
			}

			if ( Trace.Ray( Gadget.Position, Gadget.Position + Gadget.Velocity ).Ignore( Gadget ).Run().Hit )
			{
				_explosiveComponent?.ExplodeAfterSeconds( _explosiveComponent.ExplodeAfter );
			}
		}
	}

	private void DrawSegments()
	{
		foreach ( var segment in Segments )
			DebugOverlay.Line( segment.StartPos, segment.EndPos, Game.IsServer ? Color.Red : Color.Green );
	}
}
