using Grubs.Helpers;

namespace Grubs.Equipment.Weapons;

[Title( "Grub - Explosive Projectile" )]
[Category( "Equipment" )]
public class ExplosiveProjectileComponent : Component
{
	[Property] public float ExplosionDamage { get; set; } = 50f;
	[Property] public bool ExplodeOnCollision { get; set; } = false;
	[Property] public bool DeleteOnExplode { get; set; } = true;
	[Property] [Sync] public float ExplodeAfter { get; set; } = 4.0f;

	public void Explode()
	{
		Sound.Play( "explosion_short_tail" );
		ExplosionHelperComponent.Instance.Explode( this, Transform.Position, 100f, 50f );
		GameObject.Destroy();
	}
}
