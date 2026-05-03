namespace Grubs.Equipment.Weapons;

/// <summary>
/// Drop-in replacement for the Action Graph OnFire delegate on Weapon.
/// Add this component alongside a Weapon component on any weapon prefab,
/// set the properties to match what the old graph did, and remove the graph.
/// </summary>
[Title( "Grubs - Projectile Launcher" ), Category( "Equipment" )]
public sealed class ProjectileLauncher : Component
{
    public enum FireFinishedMode
    {
        /// <summary>Let the projectile resolve itself (e.g. grenade, bazooka).</summary>
        WhenProjectileResolved,
        /// <summary>Call FireFinished immediately after spawning (e.g. landmine, dynamite).</summary>
        Immediately,
    }

    [Property] public required Weapon Weapon { get; set; }

    /// <summary>The projectile prefab to clone when fired.</summary>
    [Property] public GameObject ProjectilePrefab { get; set; }

    /// <summary>
    /// How many projectiles to spawn per fire. Each additional spawn is delayed
    /// by <see cref="SpawnDelay"/> seconds. Skips spawning if the weapon is no
    /// longer deployed between shots (matches elemental missile behaviour).
    /// </summary>
    [Property] public int ProjectileCount { get; set; } = 1;

    /// <summary>Seconds between each spawned projectile when ProjectileCount > 1.</summary>
    [Property] public float SpawnDelay { get; set; } = 0f;

    [Property] public FireFinishedMode FinishedMode { get; set; } = FireFinishedMode.WhenProjectileResolved;

    /// <summary>
    /// If true, the spawned projectile is expected to be a <see cref="TargetedProjectile"/>
    /// and <c>ShareData()</c> is called on it, then <c>ResetParameters()</c> is called on
    /// the <see cref="TargetingWeapon"/> (airstrike, bazooka homing, bunker buster, concrete garry).
    /// </summary>
    [Property] public bool ShareTargetData { get; set; } = false;

    /// <summary>
    /// If true, the spawned projectile is passed to
    /// <see cref="RemoteDetonateWeapon.ReceiveProjectile"/> (goat).
    /// </summary>
    [Property] public bool SendToRemoteDetonate { get; set; } = false;

    protected override void OnStart()
    {
        if ( !Weapon.IsValid() )
            return;

        Weapon.OnFire = Launch;
    }

    private async void Launch( int charge )
    {
        if ( !ProjectilePrefab.IsValid() )
            return;

        for ( var i = 0; i < ProjectileCount; i++ )
        {
            if ( i > 0 && SpawnDelay > 0f )
            {
                await Task.DelaySeconds( SpawnDelay );

                // Stop spawning if the weapon was holstered mid-sequence.
                if ( !Weapon.IsValid() || !Weapon.Equipment.IsValid() || !Weapon.Equipment.Deployed )
                {
                    CallFireFinishedIfImmediate();
                    return;
                }
            }

            if ( !this.IsValid() )
                return;

            var go = Weapon.SpawnProjectile( Weapon, ProjectilePrefab, charge );

            if ( !go.IsValid() )
                continue;

            if ( ShareTargetData )
            {
                if ( go.Components.TryGet( out Gadgets.Projectiles.TargetedProjectile targeted ) )
                    targeted.ShareData();

                if ( Components.TryGet( out TargetingWeapon targeting, FindMode.EverythingInSelfAndAncestors ) )
                    targeting.ResetParameters();
            }

            if ( SendToRemoteDetonate )
            {
                if ( Components.TryGet( out RemoteDetonateWeapon remote, FindMode.EverythingInSelfAndAncestors ) )
                    remote.ReceiveProjectile( go );
            }
        }

        CallFireFinishedIfImmediate();
    }

    private void CallFireFinishedIfImmediate()
    {
        if ( FinishedMode == FireFinishedMode.Immediately && Weapon.IsValid() )
            Weapon.FireFinished();
    }
}
