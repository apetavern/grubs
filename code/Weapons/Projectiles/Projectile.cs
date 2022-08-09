namespace Grubs.Weapons.Projectiles;

public partial class Projectile : ModelEntity
{
	private float CollisionExplosionDelaySeconds { get; set; }

	public Projectile WithModel( string modelPath )
	{
		SetModel( modelPath );
		return this;
	}

	public Projectile SetPosition( Vector3 position )
	{
		Position = position.WithY( 0f );
		return this;
	}

	public Projectile WithCollisionExplosionDelay( float delaySeconds )
	{
		CollisionExplosionDelaySeconds = delaySeconds;
		return this;
	}

	[Event.Tick.Server]
	public void Tick()
	{
		Position -= new Vector3( 0, 0, 1f );
	}

	public void OnCollision()
	{
		if ( !IsServer )
			return;

		if ( CollisionExplosionDelaySeconds > 0 )
		{
			ExplodeAfterSeconds( CollisionExplosionDelaySeconds );
			return;
		}

		Explode();
	}

	public async void ExplodeAfterSeconds( float seconds )
	{
		await GameTask.DelaySeconds( seconds );
		Explode();
	}

	public void Explode()
	{
		Log.Info( "EXPLODE!" );
		Delete();
	}
}
