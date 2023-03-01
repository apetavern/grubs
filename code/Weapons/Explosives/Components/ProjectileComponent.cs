namespace Grubs;

[Prefab]
public partial class ProjectileComponent : ExplosiveComponent
{
	[Prefab]
	public bool ProjectileShouldUseTrace { get; set; } = false;

	[Prefab]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	public override bool ShouldStart()
	{
		return true;
	}
}
