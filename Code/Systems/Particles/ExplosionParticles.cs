namespace Grubs.Systems.Particles;

[Title( "Grubs - Explosion Particles" )]
public sealed class ExplosionParticles : Component
{
	private const string ExplosionParticlesPath = "particles/explosion/explosion.prefab";

	[Property] public float Lifetime { get; set; } = 4f;

	protected override void OnStart()
	{
		_ = DestroyAfterLifetime();
	}

	private async Task DestroyAfterLifetime()
	{
		await Task.DelaySeconds( Lifetime );
		if ( GameObject.IsValid() )
			GameObject.Destroy();
	}

	public static ExplosionParticles Spawn()
	{
		var go = GameObject.Clone( ExplosionParticlesPath );
		var explosion = go.Components.Get<ExplosionParticles>();
		return explosion;
	}

	public ExplosionParticles SetWorldPosition( Vector3 position )
	{
		WorldPosition = position;
		return this;
	}

	public ExplosionParticles SetScale( float scale )
	{
		const float explosionRelativeScale = 325f;
		var realScale = scale / explosionRelativeScale;
		
		var spriteRenderers = Components.GetAll<ParticleSpriteRenderer>( FindMode.EnabledInSelfAndChildren );
		foreach ( var spriteRenderer in spriteRenderers )
		{
			spriteRenderer.Scale = realScale;
		}

		var modelRenderers = Components.GetAll<ParticleModelRenderer>( FindMode.EnabledInSelfAndChildren );
		foreach ( var modelRenderer in modelRenderers )
		{
			modelRenderer.Scale = realScale;
		}

		return this;
	}
}
