namespace Grubs;

public static class FireHelper
{
	public static void StartFiresAt( Vector3 position, Vector3 moveDirection, int fireQuantity )
	{
		Game.AssertServer();

		for ( var i = 0; i < fireQuantity * 5; i++ )
		{
			var baseDirection = Vector3.Random.WithY( 0f ) * 45f;
			_ = new FireEntity(
				position + Vector3.Random.WithY( 0f ) * 45f,
				baseDirection * moveDirection );
		}
	}

	public static void StartFiresWithDirection( Vector3 position, Vector3 moveDirection, int fireQuantity )
	{
		Game.AssertServer();

		for ( var i = 0; i < fireQuantity * 3; i++ )
		{
			_ = new FireEntity(
				position + moveDirection.Normal * 3f,
				moveDirection * Game.Random.Float( 0.8f, 1f ),
				Game.PhysicsWorld.Gravity * Time.Delta / 2f );
		}
	}
}

[Category( "Weapons" )]
public class FireEntity : ModelEntity, IResolvable
{
	public bool Resolved => _timeUntilExpire;
	public Vector3 Gravity;

	private const float fireSize = 7.5f;
	private Particles FireParticle { get; set; }
	private Vector3 MoveDirection { get; set; }
	private TimeUntil _timeUntilExpire;
	private TimeSince _timeSinceSubtraction;
	private Sound _burningSound;

	public FireEntity()
	{

	}

	public FireEntity( Vector3 startPosition, Vector3 moveDirection )
	{
		Position = startPosition;
		MoveDirection = moveDirection;
		Velocity = MoveDirection * Time.Delta * 10f;

		Gravity = Game.PhysicsWorld.Gravity * Time.Delta / 10f;
	}

	public FireEntity( Vector3 startPosition, Vector3 moveDirection, Vector3 gravity )
	{
		Position = startPosition;
		MoveDirection = moveDirection;
		Velocity = MoveDirection * Time.Delta * 10f;

		if ( gravity == Vector3.Zero )
		{
			Gravity = Game.PhysicsWorld.Gravity * Time.Delta / 10f;
		}
		else
		{
			Gravity = gravity;
		}
	}

	public override void Spawn()
	{
		Transmit = TransmitType.Always;
		FireParticle = Particles.Create( "particles/fire/fire_base.vpcf", this, true );
		FireParticle.SetPosition( 1, (float)(_timeUntilExpire = Game.Random.Float( 0.5f, 2.5f )) );
		Health = 1;
		Tags.Add( Tag.Fire );
		Name = "fire";
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Position, fireSize );
	}

	[GameEvent.Tick.Server]
	private void Tick()
	{
		if ( !_burningSound.IsPlaying )
			_burningSound = PlaySound( "fire" );

		if ( _timeUntilExpire )
		{
			_burningSound.Stop();
			FireParticle.Destroy();
			FireParticle = null;
			Delete();
		}

		Move();
	}

	private void Move()
	{
		Velocity += MoveDirection * Time.Delta / 2f;
		Velocity += Gravity;
		Velocity += GamemodeSystem.Instance.ActiveWindForce * 128f * Time.Delta;

		Velocity = Velocity.WithY( 0f );

		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace
			.Size( fireSize )
			.WithAnyTags( Tag.Solid, Tag.Gadget )
			.WithoutTags( Tag.Fire, Tag.Player );

		mover.TryMove( Time.Delta );
		Velocity = mover.Velocity;
		Position = mover.Position;

		FireParticle?.SetPosition( 0, Position );

		var collisionTrace = Trace.Sphere( fireSize / 2, Position, Position + Vector3.Down * 5f )
			.WithAnyTags( Tag.Solid, Tag.Gadget, Tag.Player )
			.Run();

		if ( collisionTrace.Hit )
		{
			Velocity *= 0.95f;

			MoveDirection *= 0.95f;

			if ( collisionTrace.Entity is not null && !collisionTrace.Entity.Tags.Has( Tag.Invincible ) )
				collisionTrace.Entity.TakeDamage( DamageInfoExtension.FromExplosion( 0.25f, Position.WithY( 0f ), Vector3.Up * 32f, this ) );

			if ( collisionTrace.Entity is Grub grub )
				grub.ApplyAbsoluteImpulse( ((grub.Position - Position).Normal * 32f).WithY( 0f ) );
		}

		if ( _timeSinceSubtraction > 0.1f )
		{
			var terrain = GrubsGame.Instance.Terrain;
			var materials = terrain.GetActiveMaterials( MaterialsConfig.Destruction );
			terrain.SubtractCircle( new Vector2( Position.x, Position.z ), fireSize, materials );
			terrain.ScorchCircle( new Vector2( Position.x, Position.z ), fireSize + 4f );
			_timeSinceSubtraction = 0f;
		}

	}
}
