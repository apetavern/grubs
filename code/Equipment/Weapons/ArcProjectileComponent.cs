using Grubs.Helpers;

namespace Grubs.Equipment.Weapons;

[Title( "Grub - Arc Projectile" )]
[Category( "Equipment" )]
public class ArcProjectileComponent : ProjectileComponent
{
	[Property] public bool ShouldBounce { get; set; }
	[Property] public int MaxBounces { get; set; }

	private List<ArcSegment> Segments = new();
	private float _alpha;

	protected override void OnStart()
	{
		if ( Source is null )
			return;

		Transform.Position = Source.GetStartPosition();

		if ( PlayerController is null )
			return;
		var dir = PlayerController.EyeRotation.Forward.Normal * PlayerController.Facing;
		Segments = CalculateTrajectory( dir, Charge );
		Transform.Position = Segments[0].StartPos;
	}

	protected override void OnFixedUpdate()
	{
		_alpha += Time.Delta * ProjectileSpeed;

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

		ViewReady();
	}

	private void UpdateFromArcSegment( ArcSegment segment, float alpha )
	{
		Transform.Rotation = Rotation.LookAt( segment.EndPos - segment.StartPos );
		Transform.Position = Vector3.Lerp( segment.StartPos, segment.EndPos, alpha ).WithY( 512f );
	}

	private List<ArcSegment> CalculateTrajectory( Vector3 direction, int charge )
	{
		if ( Grub is null )
			return Segments;

		var force = charge * 0.25f;
		var arcTrace = new ArcTrace( Grub, Transform.Position );
		return ShouldBounce
			? arcTrace.RunTowardsWithBounces( Scene, direction, force, 0f, MaxBounces )
			: arcTrace.RunTowards( Scene, direction, force, 0f );
	}
}
