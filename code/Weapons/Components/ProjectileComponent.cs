namespace Grubs;

[Prefab]
public partial class ProjectileComponent : WeaponComponent
{
	[Prefab]
	public Model ProjectileModel { get; set; }

	[Prefab]
	public bool ProjectileShouldBounce { get; set; } = false;

	[Prefab]
	public bool ProjectileShouldRotate { get; set; } = true;

	[Prefab]
	public bool ProjectileShouldUseTrace { get; set; } = false;

	[Prefab]
	public bool ProjectileShouldUseModelCollision { get; set; } = false;

	[Prefab]
	public int ProjectileMaxBounces { get; set; } = 0;

	[Prefab]
	public float ProjectileSpeed { get; set; } = 1000.0f;

	[Prefab]
	public float ProjectileExplosionRadius { get; set; } = 100.0f;

	[Prefab]
	public float ProjectileExplodeAfter { get; set; } = 4.0f;

	[Prefab]
	public float ProjectileForceMultiplier { get; set; } = 1.0f;

	[Prefab]
	public float ProjectileRadius { get; set; } = 1.0f;

	[Prefab, ResourceType( "sound" )]
	public string ProjectileExplosionSound { get; set; }

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

	public override void FireCursor()
	{
		Log.Info( "Fire Cursor" );

		if ( !Game.IsServer )
			return;

		var position = Grub.Player.MousePosition.WithZ( 1000 );

		var projectile = new Projectile()
			.WithGrub( Grub )
			.WithModel( ProjectileModel )
			.WithPosition( position )
			.WithSpeed( ProjectileSpeed )
			.WithExplosionSound( ProjectileExplosionSound )
			.WithExplosionRadius( ProjectileExplosionRadius )
			.WithBouncing( ProjectileShouldBounce )
			.WithRotate( ProjectileShouldRotate )
			.SetCollisionReaction( ProjectileCollisionReaction.Explosive );

		if ( ProjectileShouldUseTrace )
		{
			var arcTrace = new ArcTrace( Grub, Grub.Player.MousePosition.WithZ( 1000 ) );
			var segments = ProjectileShouldBounce
				? arcTrace.RunTowardsWithBounces( -Vector3.Up, ProjectileForceMultiplier * Charge, 0, ProjectileMaxBounces )
				: arcTrace.RunTowards( -Vector3.Up, ProjectileForceMultiplier * Charge, 0f );

			projectile.MoveAlongTrace( segments );
		}
		else
		{
			if ( ProjectileShouldUseModelCollision )
			{
				projectile.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			}
			else
			{
				projectile.SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, position, ProjectileRadius );
			}
			var desiredPosition = position - Vector3.Up * 40f;
			var tr = Trace.Ray( position, desiredPosition ).Ignore( projectile ).Ignore( Weapon.Owner ).Run();
			projectile.Position = tr.EndPosition;
			projectile.Velocity = (-Vector3.Up * ProjectileSpeed).WithY( 0f );
		}

		if ( ProjectileExplodeAfter > 0 )
			projectile.ExplodeAfterSeconds( ProjectileExplodeAfter );

		projectile.Finish();
		Grub.SetAnimParameter( "fire", true );
		IsFiring = false;
		FireFinished();
	}

	public override void FireInstant()
	{
		Log.Info( "Fire Instant" );
	}

	public override void FireCharged()
	{
		Log.Info( "Fire Charged: " + Charge );

		if ( !Game.IsServer )
			return;

		var position = Weapon.Position.WithY( 0f );
		var muzzle = Weapon.GetAttachment( "muzzle" );
		if ( muzzle is not null )
			position = muzzle.Value.Position.WithY( 0f );

		var projectile = new Projectile()
			.WithGrub( Grub )
			.WithModel( ProjectileModel )
			.WithPosition( position )
			.WithSpeed( ProjectileSpeed )
			.WithExplosionSound( ProjectileExplosionSound )
			.WithExplosionRadius( ProjectileExplosionRadius )
			.SetCollisionReaction( ProjectileCollisionReaction.Explosive );

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
			var tr = Trace.Ray( position, desiredPosition ).Ignore( Weapon.Owner ).Run();
			projectile.Position = tr.EndPosition;
			projectile.Velocity = (Grub.EyeRotation.Forward.Normal * Grub.Facing * Charge * ProjectileSpeed).WithY( 0f );
		}

		if ( ProjectileExplodeAfter > 0 )
			projectile.ExplodeAfterSeconds( ProjectileExplodeAfter );

		projectile.Finish();
		Grub.SetAnimParameter( "fire", true );

		IsFiring = false;
		Charge = 0;

		FireFinished();
	}
}
