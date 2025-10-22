namespace Grubs.Systems.Particles;

[Title( "Grubs - Tracer Particles" )]
public sealed class TracerParticles : Component
{
	private const string TracerParticlesPath = "particles/guntrace/guntrace.prefab";
	private const string RailgunParticlesPath = "particles/guntrace/railgun.prefab";

	public enum TracerEffectType
	{
		Normal,
		Railgun
	}

	public static TracerParticles Spawn( TracerEffectType type = TracerEffectType.Normal )
	{
		var path = type switch
		{
			TracerEffectType.Railgun => RailgunParticlesPath,
			_ => TracerParticlesPath
		};

		var go = GameObject.Clone( path );
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
