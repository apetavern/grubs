namespace Grubs.Helpers;

[Title( "Grubs - Particle Helper" ), Category( "World" )]
public sealed class ParticleHelper : Component
{
	public static ParticleHelper Instance { get; set; } = new();
	private List<SceneParticles> _sceneObjects = new();

	public ParticleHelper()
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

	public void Dispose( SceneParticles sceneParticles )
	{
		_sceneObjects.Remove( sceneParticles );
		sceneParticles?.Delete();
	}

	protected override void OnUpdate()
	{
		_sceneObjects.RemoveAll( s => s.Finished );

		foreach ( var sceneObject in _sceneObjects )
		{
			sceneObject?.Simulate( Time.Delta );
		}
	}
}
