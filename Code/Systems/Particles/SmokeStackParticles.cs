namespace Grubs.Systems.Particles;

[Title( "Grubs - Smoke Stack Particles" )]
public sealed class SmokeStackParticles : Component
{
	private const string SmokeStackParticlesPath = "particles/smoke/smoke.prefab";

	public static SmokeStackParticles Spawn()
	{
		var go = GameObject.Clone( SmokeStackParticlesPath );
		var particles = go.Components.Get<SmokeStackParticles>();
		return particles;
	}

	public SmokeStackParticles SetWorldPosition( Vector3 position )
	{
		WorldPosition = position;
		return this;
	}
}
