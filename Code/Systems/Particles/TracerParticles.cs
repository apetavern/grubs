namespace Grubs.Systems.Particles;

[Title( "Grubs - Tracer Particles" )]
public sealed class TracerParticles : Component
{
	private const string TracerParticlesPath = "particles/guntrace/guntrace.prefab";
	private const string RailgunParticlesPath = "particles/guntrace/railgun.prefab";

	public static TracerParticles Spawn( bool railgun = false )
	{
		var go = GameObject.Clone( railgun ? RailgunParticlesPath : TracerParticlesPath );
		var particles = go.Components.Get<TracerParticles>();
		return particles;
	}

	public TracerParticles SetWorldPosition( Vector3 position )
	{
		WorldPosition = position;
		return this;
	}

	public TracerParticles SetEndPoint( Vector3 position )
	{
		GameObject.Children.First().WorldPosition = position;
		return this;
	}
}
