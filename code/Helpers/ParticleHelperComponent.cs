namespace Grubs.Helpers;

[Title( "Grubs - Particle Helper" ), Category( "World" )]
public sealed class ParticleHelperComponent : Component
{
	public static ParticleHelperComponent Instance { get; set; } = new();
	private List<SceneParticles> _sceneObjects = new();

	public ParticleHelperComponent()
	{
		Instance = this;
	}

	public SceneParticles PlayInstantaneous( ParticleSystem particles )
	{
		return PlayInstantaneous( particles, global::Transform.Zero );
	}

	public SceneParticles PlayInstantaneous( ParticleSystem particles, Transform transform )
	{
		var sceneObject = new SceneParticles( Scene.SceneWorld, particles );
		sceneObject.SetControlPoint( 0, transform );
		sceneObject.Transform = transform;

		_sceneObjects.Add( sceneObject );
		return sceneObject;
	}

	protected override void OnUpdate()
	{
		_sceneObjects.RemoveAll( s => s.Finished );

		foreach ( var sceneObject in _sceneObjects )
		{
			sceneObject.Simulate( Time.Delta );
		}
	}
}
