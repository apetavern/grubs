using Grubs.Equipment.Weapons;

namespace Grubs.Common;

public partial class HealthComponent : Component
{
	[Property] public float MaxHealth { get; set; }

	[Sync] public float CurrentHealth { get; set; }

	public bool DeathInvoked { get; set; } = false;

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage( float damage )
	{
		CurrentHealth -= damage;
		if ( CurrentHealth <= 0 && !DeathInvoked )
		{
			DeathInvoked = true;

			if ( Components.TryGet( out ExplosiveProjectileComponent explosive ) && explosive.ExplodeOnDeath )
			{
				explosive.Explode();
			}
		}
	}

	public void Heal( float heal )
	{
		CurrentHealth += heal;
	}
}
