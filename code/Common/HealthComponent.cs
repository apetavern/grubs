using Grubs.Equipment.Weapons;
using Grubs.Gamemodes;
using Grubs.Pawn;

namespace Grubs.Common;

public partial class HealthComponent : Component
{
	[Property] public float MaxHealth { get; set; }

	[Sync] public float CurrentHealth { get; set; }

	public bool DeathInvoked { get; set; } = false;
	private Queue<float> DamageQueue { get; set; } = new();

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage( float damage, bool immediate = false )
	{
		if ( Components.TryGet( out Grub grub ) && !immediate )
		{
			if ( Connection.Local.IsHost && !Gamemode.Current.DamageQueue.Contains( grub ) )
				Gamemode.Current.DamageQueue.Enqueue( grub );

			DamageQueue.Enqueue( damage );
			return;
		}

		CurrentHealth -= damage;
		if ( CurrentHealth <= 0 && !DeathInvoked )
		{
			DeathInvoked = true;

			if ( Components.TryGet( out ExplosiveProjectileComponent explosive ) && explosive.ExplodeOnDeath )
			{
				explosive.Explode();
			}

			if ( grub.IsValid() )
			{
				var conn = grub.Network.OwnerConnection;
				grub.GameObject.Destroy();
				// NetHelper.Instance.OnActive( conn );
			}
		}
	}

	/// <summary>
	/// Will dequeue DamageQueue until empty and apply any damage to CurrentHealth.
	/// </summary>
	/// <returns></returns>
	[Broadcast]
	public void ApplyDamage()
	{
		if ( !DamageQueue.Any() )
			return;

		var totalDamage = 0f;
		while ( DamageQueue.TryDequeue( out var damage ) )
			totalDamage += damage;

		TakeDamage( totalDamage, true );
	}

	public void Heal( float heal )
	{
		CurrentHealth += heal;
	}
}
