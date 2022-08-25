using Grubs.Player;
using Grubs.Utils.Extensions;

namespace Grubs.Utils;

public static class ExplosionHelper
{
	public static void Explode( Vector3 position, Worm source, float radius = 100, float maxDamage = 100 )
	{
		Host.AssertServer();

		var sourcePos = position;
		foreach ( var worm in Entity.All.OfType<Worm>().Where( x => Vector3.DistanceBetween( sourcePos, x.Position ) <= radius ) )
		{
			if ( !worm.IsValid() || worm.LifeState != LifeState.Alive )
				continue;

			var dist = Vector3.DistanceBetween( position, worm.Position );
			if ( dist > radius )
				continue;

			var distanceFactor = 1.0f - Math.Clamp( dist / radius, 0, 1 );
			var force = distanceFactor * 1000; // TODO: PhysicsGroup/Body is invalid on worms

			var dir = (worm.Position - position).Normal;
			worm.ApplyAbsoluteImpulse( dir * force );

			if ( !GameConfig.FriendlyFire && worm != source && worm.TeamNumber == source.TeamNumber )
				continue;

			worm.TakeDamage( DamageInfoExtension.FromProjectile( maxDamage * distanceFactor, position, Vector3.Up * 32, source ) );
		}

		var midpoint = new Vector3( position.x, position.z );
		var size = MathX.FloorToInt( (float)Math.Sqrt( radius ) );
		GrubsGame.Current.TerrainMap.DestructSphere( midpoint, size );
		GrubsGame.ExplodeClient( To.Everyone, midpoint, size );

		GrubsGame.Current.RegenerateMap();
		DebugOverlay.Sphere( position, radius, Color.Red, 5 );
	}
}
