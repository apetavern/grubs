using Grubs.Equipment.Weapons;
using Grubs.Helpers;
using Grubs.Player;

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

			if ( Components.TryGet( out Grub grub ) )
			{
				var conn = grub.Network.OwnerConnection;
				grub.GameObject.Destroy();
				// NetHelper.Instance.OnActive( conn );
			}
		}
	}

	public void Heal( float heal )
	{
		CurrentHealth += heal;
	}
}
