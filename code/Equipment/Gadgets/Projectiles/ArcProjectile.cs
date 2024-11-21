using Grubs.Equipment.Weapons;
using Grubs.Helpers;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Arc Projectile" ), Category( "Equipment" )]
public class ArcProjectile : Projectile
{
	[Property] public bool ShouldBounce { get; set; }
	[Property] public int MaxBounces { get; set; }
	[Property] public ExplosiveProjectile Explosive { get; set; }

	public override bool Resolved => !Segments.Any();

	private List<ArcSegment> Segments = new();
	private float _alpha;

	protected override void OnStart()
	{
		if ( !Source.IsValid() )
			return;

		WorldPosition = Source.GetStartPosition();

		if ( !PlayerController.IsValid() )
			return;

		var dir = PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing;
		Segments = CalculateTrajectory( dir, Charge );
		if ( Segments.Count == 0 )
			return;
		WorldPosition = Segments[0].StartPos.WithY( 512f );
		Transform.ClearInterpolation();
	}

	private bool secondUpdate;

	protected override void OnFixedUpdate()
	{
		// We are seeing the model for the projectile very quickly moving from its spawn location
		// to the proper start position unless we wait until the second physics tick to render it.
		if ( secondUpdate && !Model.Enabled )
			ViewReady();

		secondUpdate = true;

		// We want to evenly increase our alpha based on how many updates we have.
		// (if Time.Delta * ProjectileSpeed = 0.7, we want an even two 0.5 lerps, not 0.7 -> 0.3
		var wholeIterations = MathF.Floor( 1f / (Time.Delta * ProjectileSpeed) ) + 1;
		_alpha += 1 / wholeIterations;

		if ( Segments.Any() )
		{
			var currentSegment = Segments.FirstOrDefault();

			if ( _alpha >= 1f )
			{
				if ( Segments.Count == 1 )
				{
					UpdateFromArcSegment( currentSegment, 1f );
					Segments.RemoveAt( 0 );
					return;
				}

				_alpha = 0;
				Segments.RemoveAt( 0 );
			}

			currentSegment = Segments.FirstOrDefault();
			UpdateFromArcSegment( currentSegment, _alpha );
		}
		else
		{
			Explosive?.Explode();
		}
	}

	private void UpdateFromArcSegment( ArcSegment segment, float alpha )
	{
		WorldRotation = Rotation.Slerp( WorldRotation, Rotation.LookAt( segment.EndPos - segment.StartPos ),
			alpha );
		WorldPosition = Vector3.Lerp( segment.StartPos, segment.EndPos, alpha ).WithY( 512f );
	}

	private List<ArcSegment> CalculateTrajectory( Vector3 direction, int charge )
	{
		if ( !Grub.IsValid() )
			return Segments;

		var force = charge * 0.5f;
		var arcTrace = new ArcTrace( Grub, WorldPosition );
		return ShouldBounce
			? arcTrace.RunTowardsWithBounces( Scene, direction, force, 0f, MaxBounces )
			: arcTrace.RunTowards( Scene, direction, force, 0f );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Transform = global::Transform.Zero.WithScale( 1f );

		foreach ( var segment in Segments )
		{
			Gizmo.Draw.Line( segment.StartPos.WithY( 512f ), segment.EndPos.WithY( 512f ) );
		}
	}
}
