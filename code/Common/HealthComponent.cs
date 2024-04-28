using Grubs.Equipment.Weapons;
using Grubs.Gamemodes;
using Grubs.Helpers;
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
			OnDeath();
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

	private async Task OnDeath()
	{
		if ( Components.TryGet( out Grub grub ) )
		{
			await GameTask.Delay( 500 ); // Give clients some time to update GrubTag healthbar to 0 before we play death animation.
			DeathInvoked = true;

			// This is shit, especially since we want a variety of death animations in the future.
			// Just don't know where to put this right now.
			var prefab = ResourceLibrary.Get<PrefabFile>( "prefabs/world/dynamite_plunger.prefab" );
			var plunger = SceneUtility.GetPrefabScene( prefab ).Clone();
			var position = grub.Transform.Position;
			plunger.Transform.Position = grub.PlayerController.Facing == -1 ? position - new Vector3( 30, 0, 0 ) : position;

			await GameTask.Delay( 750 );

			// Same as above.
			var sceneParticles = ParticleHelperComponent.Instance.PlayInstantaneous( ParticleSystem.Load( "particles/explosion/grubs_explosion_base.vpcf" ), Transform.World );
			sceneParticles.SetControlPoint( 1, new Vector3( 100f / 2f, 0, 0 ) );
			Sound.Play( "explosion_short_tail", position );

			ExplosionHelperComponent.Instance.Explode( grub, position, 100f, 25f );

			grub.GameObject.Destroy();
			plunger?.Destroy();
			return;
		}

		DeathInvoked = true;

		if ( Components.TryGet( out ExplosiveProjectileComponent explosive ) && explosive.ExplodeOnDeath )
		{
			explosive.Explode();
		}
	}
}
