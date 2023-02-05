namespace Grubs;

[Prefab]
public class ProjectileComponent : WeaponComponent
{
	[Prefab]
	public Model ProjectileModel { get; set; }

	[Prefab]
	public bool ProjectileShouldBounce { get; set; } = false;

	[Prefab]
	public int ProjectileMaxBounces { get; set; } = 0;

	[Prefab]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	[Prefab]
	public float ProjectileExplosionRadius { get; set; } = 100.0f;

	[Prefab]
	public float ProjectileForceMultiplier { get; set; } = 1.0f;

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			Log.Info( "Grubs: ProjectileComponent::Simulate FIRE!" );
		}
	}
}
