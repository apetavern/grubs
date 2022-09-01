using Grubs.Player;
using Grubs.Utils;

namespace Grubs.Weapons.Base;

/// <summary>
/// A weapon capable of firing projectiles.
/// </summary>
public abstract class ProjectileWeapon : GrubWeapon
{
	public override bool HasReticle => true;

	/// <summary>
	/// The base force at which the projectile will be propelled in the arc trace.
	/// </summary>
	protected virtual float ProjectileForceMultiplier => 1;
	/// <summary>
	/// Whether or not this projectile should bounce.
	/// </summary>
	protected virtual bool ProjectileShouldBounce => false;
	/// <summary>
	/// The max amount of bounces this projectile can do.
	/// <remarks>This will be unused if <see cref="ProjectileShouldBounce"/> is false.</remarks>
	/// </summary>
	protected virtual int ProjectileMaxBounces => 0;
	/// <summary>
	/// The model of the projectile.
	/// </summary>
	protected virtual string ProjectileModel => string.Empty;
	/// <summary>
	/// The speed of the projectile while it is following the arc trace.
	/// </summary>
	protected virtual float ProjectileSpeed => 1000;
	/// <summary>
	/// The radius of the explosion created by the projectile.
	/// </summary>
	protected virtual float ProjectileExplosionRadius => 100;
	/// <summary>
	/// The amount of seconds before the projectile will automatically explode.
	/// <remarks>This will only work if the value is greater than 0.</remarks>
	/// </summary>
	protected virtual float ProjectileExplodeAfter => 0;
	/// <summary>
	/// The amount of seconds before the projectile will automatically explode after colliding.
	/// <remarks>This will only work if the value is greater than 0.</remarks>
	/// </summary>
	protected virtual float ProjectileCollisionExplosionDelay => 0;

	protected override void OnFire()
	{
		base.OnFire();

		var arcTrace = new ArcTrace( Parent, Parent.EyePosition );
		var segments = ProjectileShouldBounce
			? arcTrace.RunTowardsWithBounces( Parent.EyeRotation.Forward.Normal, ProjectileForceMultiplier * Charge, 0, ProjectileMaxBounces )
			: arcTrace.RunTowards( Parent.EyeRotation.Forward.Normal, ProjectileForceMultiplier * Charge, 0 );

		var projectile = new Projectile()
			.WithGrub( (Parent as Grub)! )
			.WithModel( ProjectileModel )
			.WithPosition( Position )
			.WithSpeed( ProjectileSpeed )
			.WithExplosionRadius( ProjectileExplosionRadius )
			.MoveAlongTrace( segments )
			.Finish();

		if ( ProjectileCollisionExplosionDelay > 0 )
			projectile.WithCollisionExplosionDelay( ProjectileCollisionExplosionDelay );

		if ( ProjectileExplodeAfter > 0 )
			projectile.ExplodeAfterSeconds( 5f );

		SetupProjectile( projectile );
		GrubsCamera.SetTarget( projectile );
	}

	/// <summary>
	/// Called to handle any extra logic needed for the projectile.
	/// </summary>
	/// <param name="projectile">The projectile to edit.</param>
	protected virtual void SetupProjectile( Projectile projectile )
	{
	}
}
