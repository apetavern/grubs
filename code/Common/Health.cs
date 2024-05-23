using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Equipment.Weapons;
using Grubs.Gamemodes;
using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Common;

public partial class Health : Component
{
	[Property] public float MaxHealth { get; set; }

	[Sync] public float CurrentHealth { get; set; }

	[Sync] public bool DeathInvoked { get; set; } = false;

	public delegate void Death();

	public event Death ObjectDied;

	public delegate void Damaged( GrubsDamageInfo damageInfo );

	public event Damaged ObjectDamaged;

	private Queue<GrubsDamageInfo> DamageQueue { get; set; } = new();

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage( GrubsDamageInfo damageInfo, bool immediate = false )
	{
		if ( Components.TryGet( out Grub grub ) && !immediate )
		{
			if ( Connection.Local.IsHost && !Gamemode.Current.DamageQueue.Contains( grub ) )
				Gamemode.Current.DamageQueue.Enqueue( grub );

			DamageQueue.Enqueue( damageInfo );
			return;
		}
		
		if (grub.IsValid() && grub.IsActive && immediate )
			Gamemode.FFA.UseTurn();

		CurrentHealth -= damageInfo.Damage;

		ObjectDamaged?.Invoke( damageInfo );

		if ( CurrentHealth <= 0 && !DeathInvoked )
		{
			_ = OnDeath( damageInfo.Tags.Has( "killzone" ) );
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
		while ( DamageQueue.TryDequeue( out var info ) )
			totalDamage += info.Damage;

		TakeDamage( new GrubsDamageInfo( totalDamage ), true );
	}

	public void Heal( float heal )
	{
		CurrentHealth += heal;
	}

	private async Task OnDeath( bool deleteImmediately = false )
	{
		if ( Components.TryGet( out Grub grub ) )
		{
			if ( !deleteImmediately )
			{
				await GameTask.Delay( 500 ); // Give clients some time to update GrubTag healthbar to 0 before we play death animation.
				DeathInvoked = true;

				// This is shit, especially since we want a variety of death animations in the future.
				// Just don't know where to put this right now.
				var prefab = ResourceLibrary.Get<PrefabFile>( "prefabs/world/dynamite_plunger.prefab" );
				var position = grub.Transform.Position;
				var plunger = SceneUtility.GetPrefabScene( prefab ).Clone();
				plunger.NetworkSpawn();
				plunger.Transform.Position = grub.PlayerController.Facing == -1 ? position - new Vector3( 30, 0, 0 ) : position;

				await GameTask.Delay( 750 );

				// Same as above.
				DeathEffects( position );

				ExplosionHelper.Instance.Explode( grub, position, 100f, 25f );
				plunger?.Destroy();
			}

			grub.GameObject.Destroy();
			return;
		}

		ObjectDied?.Invoke();

		DeathInvoked = true;

		if ( Components.TryGet( out ExplosiveProjectile explosive ) && explosive.ExplodeOnDeath )
		{
			explosive.Explode();
		}
	}

	[Broadcast]
	private void DeathEffects( Vector3 position )
	{
		var sceneParticles = ParticleHelper.Instance.PlayInstantaneous( ParticleSystem.Load( "particles/explosion/grubs_explosion_base.vpcf" ), Transform.World );
		sceneParticles.SetControlPoint( 1, new Vector3( 100f / 2f, 0, 0 ) );
		Sound.Play( "explosion_short_tail", position );
	}
}
