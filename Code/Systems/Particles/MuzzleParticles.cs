namespace Grubs.Systems.Particles;

[Title( "Grubs - Muzzle Particles" )]
public sealed class MuzzleParticles : Component
{
	private const string MuzzleParticlesPath = "particles/muzzleflash/muzzleflash.prefab";

	public static MuzzleParticles Spawn()
	{
		var go = GameObject.Clone( MuzzleParticlesPath );
		var particles = go.Components.Get<MuzzleParticles>();
		return particles;
	}

	public MuzzleParticles SetWorldPosition( Vector3 position )
	{
		WorldPosition = position;
		return this;
	}

	public MuzzleParticles SetWorldRotation( Rotation rotation )
	{
		WorldRotation = rotation;
		return this;
	}
}
