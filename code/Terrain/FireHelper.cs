namespace Grubs;

public static class FireHelper
{
	public static void StartFiresAt( Vector3 position, Vector3 moveDirection, int fireQuantity )
	{
		Game.AssertServer();

		for ( var i = 0; i < fireQuantity; i++ )
		{
			var baseDirection = Vector3.Random.WithY( 0f ) * 45f;
			_ = new FireEntity(
				position + Vector3.Random.WithY( 0f ) * 45f,
				baseDirection * moveDirection );
		}
	}
}

[Category( "Weapons" )]
public class FireEntity : ModelEntity, IResolvable
{
	public bool Resolved => Time.Now > _expiryTime;
	private Vector3 _moveDirection { get; set; }

	private readonly float _expiryTime;
	private const float fireSize = 10f;

	private Particles FireParticle { get; set; }

	public FireEntity()
	{

	}

	public FireEntity( Vector3 startPosition, Vector3 moveDirection )
	{
		Position = startPosition;
		_moveDirection = moveDirection;

		_expiryTime = Time.Now + Game.Random.Float( 0.5f, 2.5f );
	}

	public override void Spawn()
	{
		FireParticle = Particles.Create( "particles/fire/fire_base.vpcf", this, true );
		Tags.Add( "fire" );
		Name = "fire";
	}

	[GameEvent.Tick.Server]
	private void Tick()
	{
		if ( Time.Now > _expiryTime )
		{
			FireParticle = null;
			Delete();
		}

		Move();
	}

	private void Move()
	{
		Velocity += new Vector3( _moveDirection ) * Time.Delta / 2f;
		Velocity += Game.PhysicsWorld.Gravity * Time.Delta / 10f;
		Velocity += GamemodeSystem.Instance.ActiveWindForce * 128f * Time.Delta;

		Velocity = Velocity.WithY( 0f );

		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace
			.Size( fireSize )
			.WithAnyTags( "solid", "gadget" )
			.WithoutTags( "fire", "player" );

		mover.TryMove( Time.Delta );
		Velocity = mover.Velocity;
		Position = mover.Position;

		var collisionTrace = Trace.Sphere( fireSize / 2, Position, Position + Vector3.Down * 5f )
			.WithAnyTags( "solid", "gadget", "player" )
			.Run();

		if ( collisionTrace.Hit )
		{
			if ( collisionTrace.Entity is not null && !collisionTrace.Entity.Tags.Has( "invincible" ) )
				collisionTrace.Entity.TakeDamage( DamageInfoExtension.FromExplosion( 0.25f, Position, Vector3.Up * 32f, this ) );

			if ( collisionTrace.Entity is Grub grub )
				grub.ApplyAbsoluteImpulse( (grub.Position - Position).Normal * 32f );
		}

		var terrain = GrubsGame.Instance.Terrain;
		var materials = terrain.GetActiveMaterials( MaterialsConfig.Destruction );
		terrain.SubtractCircle( new Vector2( Position.x, Position.z ), fireSize, materials );
	}
}
