namespace Grubs;

// TODO: Maybe we can have something generic here instead? Not sure yet.
// Maybe a different name for this class, it is kinda similar to the other class.
[Prefab]
public partial class ProjectileWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public string ProjectilePrefabPath { get; set; }

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

		if ( PrefabLibrary.TrySpawn<Explosive>( ProjectilePrefabPath, out var explosive ) )
		{
			// TODO: Maybe have some generic way we can pass this information into an explosive?
			// OnFire(Grub, Charge, Position, Velocity)?
			// Then we can calculate the arc trace inside of the projectile component instead of doing it here.
			if ( explosive.Components.TryGet<ProjectileComponent>( out var projectile ) )
			{
				explosive.Owner = Grub;

				var arcTrace = new ArcTrace( Grub, Grub.EyePosition );
				projectile.Segments = arcTrace.RunTowards( Grub.EyeRotation.Forward.Normal * Grub.Facing, explosive.ExplosionForceMultiplier * Charge, 0f );
				explosive.Position = projectile.Segments[0].StartPos;
			}
		}

		Grub.SetAnimParameter( "fire", true );

		IsFiring = false;
		Charge = 0;

		FireFinished();
	}
}
