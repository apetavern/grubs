namespace Grubs.Systems.Particles;

[Title( "Grubs - Poison Particles" )]
public sealed class PoisonParticles : Component
{
	private const string PoisonParticlesPath = "particles/explosion/poison.prefab";

	public static PoisonParticles Spawn()
	{
		var go = GameObject.Clone( PoisonParticlesPath );
		var explosion = go.Components.Get<PoisonParticles>();
		return explosion;
	}

	public PoisonParticles SetWorldPosition( Vector3 position )
	{
		WorldPosition = position;
		return this;
	}

	public PoisonParticles SetScale( float scale )
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
