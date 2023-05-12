namespace Grubs;

public static partial class ExplosionHelper
{
	private static Terrain Terrain => GrubsGame.Instance.Terrain;

	/// <summary>
	/// Creates an explosion at a given point where a <see cref="Grub"/> is responsible for it.
	/// </summary>
	/// <param name="position">The center point of the explosion.</param>
	/// <param name="source">The grub responsible for creating this explosion.</param>
	/// <param name="radius">The radius of the explosion.</param>
	/// <param name="maxDamage">The max amount of damage the explosion can do to a grub.</param>
	public static void Explode( Vector3 position, Grub source, float radius = 100, float maxDamage = 100 )
	{
		if ( !Game.IsServer )
			return;

		var sourcePos = position;
		foreach ( var grub in Entity.All.OfType<Grub>().Where( x => Vector3.DistanceBetween( sourcePos, x.Position ) <= radius ) )
		{
			if ( !grub.IsValid() || grub.LifeState != LifeState.Alive )
				continue;

			var dist = Vector3.DistanceBetween( position, grub.Position );
			if ( dist > radius )
				continue;

			var distanceFactor = 1.0f - Math.Clamp( dist / radius, 0, 1 );
			// TODO: PhysicsGroup/Body is invalid on grubs
			var force = distanceFactor * 1000;

			var dir = (grub.Position - position).Normal;
			dir = dir.WithY( 0f );
			grub.ApplyAbsoluteImpulse( dir * force );

			grub.TakeDamage( DamageInfoExtension.FromExplosion( maxDamage * distanceFactor, position, Vector3.Up * 32, source ) );
		}

		var materials = Terrain.GetSandMaterials();
		Terrain.SubtractCircle( new Vector2( position.x, position.z), radius, materials );

		if ( ExplosionDebug )
			DebugOverlay.Sphere( position, radius, Color.Red, 5 );
		DoExplosionEffectsAt( To.Everyone, position, radius );
	}

	/// <summary>
	/// Client receiver to do effects of the explosion.
	/// </summary>
	/// <param name="position">The center point of the explosion.</param>
	/// <param name="radius">The radius of the explosion.</param>
	[ClientRpc]
	public static void DoExplosionEffectsAt( Vector3 position, float radius )
	{
		var explosion = Particles.Create( "particles/explosion/grubs_explosion_base.vpcf", position );
		explosion.SetPosition( 1, new Vector3( radius * 1.2f, 0, 0 ) );
	}

	/// <summary>
	/// Debug console variable to show the explosions radius.
	/// </summary>
	[ConVar.Replicated( "explosion_debug" )]
	public static bool ExplosionDebug { get; set; }
}
