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
	public bool Resolved => _timeUntilExpire;
	private Vector3 _moveDirection { get; set; }

	private TimeUntil _timeUntilExpire;
	private const float fireSize = 10f;

	private Particles FireParticle { get; set; }

	public FireEntity()
	{

	}

	public FireEntity( Vector3 startPosition, Vector3 moveDirection )
	{
		Position = startPosition;
		_moveDirection = moveDirection;
		Velocity = _moveDirection * Time.Delta * 10f;
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
		if ( _timeUntilExpire )
		{
			FireParticle.Destroy();
			FireParticle = null;
			Delete();
		}

		Move();
	}

	private void Move()
	{
		Velocity += _moveDirection * Time.Delta / 2f;
		Velocity += Game.PhysicsWorld.Gravity * Time.Delta / 10f;
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
			if ( collisionTrace.Entity is not null && !collisionTrace.Entity.Tags.Has( Tag.Invincible ) )
				collisionTrace.Entity.TakeDamage( DamageInfoExtension.FromExplosion( 0.25f, Position, Vector3.Up * 32f, this ) );

			if ( collisionTrace.Entity is Grub grub )
				grub.ApplyAbsoluteImpulse( (grub.Position - Position).Normal * 32f );
		}

		var terrain = GrubsGame.Instance.Terrain;
		var materials = terrain.GetActiveMaterials( MaterialsConfig.Destruction );
		terrain.SubtractCircle( new Vector2( Position.x, Position.z ), fireSize, materials );
		terrain.ScorchCircle( new Vector2( Position.x, Position.z ), fireSize + 4f );
	}
}
