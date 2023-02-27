namespace Grubs;

[Prefab]
public partial class ProjectileComponent : WeaponComponent
{
	[Prefab]
	public Model ProjectileModel { get; set; }

	[Prefab, Net]
	public bool ProjectileShouldBounce { get; set; } = false;

	[Prefab, Net]
	public bool ProjectileShouldUseTrace { get; set; } = false;

	[Prefab, Net]
	public int ProjectileMaxBounces { get; set; } = 0;

	[Prefab, Net]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	[Prefab, Net]
	public float ProjectileExplosionRadius { get; set; } = 100.0f;

	[Prefab, Net]
	public float ProjectileExplodeAfter { get; set; } = 4.0f;

	[Prefab, Net]
	public float ProjectileForceMultiplier { get; set; } = 1.0f;

	[Prefab, Net]
	public float ProjectileRadius { get; set; } = 1.0f;

	[Prefab, ResourceType( "sound" )]
	public string ProjectileExplosionSound { get; set; }

	[Prefab, ResourceType( "vpcf" )]
	public string TrailParticle { get; set; }

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireInstant()
	{
		// Instantly fire using the minimum charge.
		FireCharged();
	}

	public override void FireCharged()
	{
		var position = Weapon.Position.WithY( 0f );
		var muzzle = Weapon.GetAttachment( "muzzle" );
		if ( muzzle is not null )
			position = muzzle.Value.Position.WithY( 0f );

		var projectile = new Projectile();

		if ( ProjectileShouldUseTrace )
		{
			var arcTrace = new ArcTrace( Grub, Grub.EyePosition );
			var segments = ProjectileShouldBounce
				? arcTrace.RunTowardsWithBounces( Grub.EyeRotation.Forward.Normal * Grub.Facing, ProjectileForceMultiplier * Charge, 0, ProjectileMaxBounces )
				: arcTrace.RunTowards( Grub.EyeRotation.Forward.Normal * Grub.Facing, ProjectileForceMultiplier * Charge, 0f );

			projectile.MoveAlongTrace( segments );
		}
		else
		{
			projectile.SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, position, ProjectileRadius );
			var desiredPosition = position + (Grub.EyeRotation.Forward.Normal * Grub.Facing * 40f);
			var tr = Trace.Ray( position, desiredPosition ).Run();
			projectile.Position = tr.EndPosition;
			projectile.Velocity = (Grub.EyeRotation.Forward.Normal * Grub.Facing * Charge * ProjectileSpeed).WithY( 0f );
		}

		if ( ProjectileExplodeAfter > 0 )
			projectile.ExplodeAfterSeconds( ProjectileExplodeAfter );

		projectile
			.WithSpeed( ProjectileSpeed )
			.WithGrub( Grub )
			.WithModel( ProjectileModel )
			.WithTrailParticle( TrailParticle )
			.WithExplosionSound( ProjectileExplosionSound )
			.WithExplosionRadius( ProjectileExplosionRadius )
			.SetCollisionReaction( ExplosiveReaction.Explosion );

		projectile.Finish();
		Grub.SetAnimParameter( "fire", true );

		IsFiring = false;
		Charge = MinCharge;

		Grub.CreatedExplosives.Add( projectile );

		FireFinished();
	}
}
