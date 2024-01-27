namespace Grubs.Common;

public class HealthComponent : Component
{
	[Property] public float MaxHealth { get; set; }

	[Sync] public float CurrentHealth { get; set; }

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage( float damage )
	{
		CurrentHealth -= damage;
	}

	public void Heal( float heal )
	{
		CurrentHealth += heal;
	}
}
