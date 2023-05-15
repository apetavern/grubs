namespace Grubs;

public static class FireHelper
{
	public static void StartFiresAt( Vector3 position, Vector3 moveDirection, int fireQuantity )
	{
		Game.AssertServer();

		var wind = GamemodeSystem.Instance.ActiveWindForce;
		var gravity = Game.PhysicsWorld.Gravity * 0.1f;

		Log.Info( wind );

		for ( var i = 0; i < fireQuantity; i++ )
		{
			var baseDirection = Vector3.Random.WithY( 0f ) * 5f;
			baseDirection += gravity;
			baseDirection = baseDirection.WithX( baseDirection.x * wind * 50f );
			_ = new FireEntity( 
				position + Vector3.Random.WithY( 0f ) * 30f, 
				baseDirection * -moveDirection );
		}
	}
}

[Category( "Weapons" )]
public class FireEntity : ModelEntity, IResolvable
{
	public bool Resolved => Time.Now > _expiryTime;
	private Vector3 _moveDirection { get; set; }

	private readonly float _expiryTime;
	private const float fireSize = 20f;

	private Particles FireParticle { get; set; }

	public FireEntity()
	{

	}

	public FireEntity( Vector3 startPosition, Vector3 moveDirection )
	{
		Position = startPosition;
		_moveDirection = moveDirection;
		Log.Info( moveDirection );
		_expiryTime = Time.Now + Game.Random.Float( 0.5f, 2.5f );
	}

	public override void Spawn()
	{
		FireParticle = Particles.Create( "particles/fire/fire_base.vpcf", this, true );
		Tags.Add( "fire" );
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

		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace
			.Size( 20f )
			.WithAnyTags( "solid", "gadget" )
			.WithoutTags( "fire", "player" );

		mover.TryMove( Time.Delta );
		Velocity = mover.Velocity;
		Position = mover.Position;

		var terrain = GrubsGame.Instance.Terrain;
		var materials = terrain.GetActiveMaterials( MaterialsConfig.Destruction );
		terrain.SubtractCircle( new Vector2(Position.x, Position.z), fireSize, materials );

		/*		var midpoint = new Vector3( _desiredPosition.x, _desiredPosition.z );
				var terrain = GrubsGame.Instance.Terrain;

				var materials = terrain.GetActiveMaterials( MaterialsConfig.Destruction );
				terrain.SubtractCircle( midpoint, fireSize, materials );

				// todo: optimize this
				var sourcePosition = _desiredPosition;
				foreach ( var grub in All.OfType<Grub>().Where( x => Vector3.DistanceBetween( sourcePosition, x.Position ) <= fireSize ) )
				{
					if ( !grub.IsValid() || grub.LifeState != LifeState.Alive )
						continue;

					var dist = Vector3.DistanceBetween( _desiredPosition, grub.Position );
					if ( dist > fireSize )
						continue;

					var distanceFactor = 1.0f - Math.Clamp( dist / fireSize, 0, 1 );
					var force = distanceFactor * 1000f;

					var dir = (grub.Position - _desiredPosition).Normal;
					grub.ApplyAbsoluteImpulse( dir * force );
					grub.TakeDamage( DamageInfoExtension.FromExplosion( 6, _desiredPosition, Vector3.Up * 32, this ) );
				}

				_desiredPosition += _moveDirection * 1.5f;
				var tr = Trace.Sphere( fireSize * 1.5f, Position, _desiredPosition ).WithAnyTags( "solid", "gadget" ).Run();
				var grounded = tr.Hit;

				if ( grounded )
				{
					_moveDirection += Vector3.Random.WithY( 0f ) * 2.5f;
					_moveDirection += tr.Normal * 0.5f;
					_moveDirection = _moveDirection * 5f;
				}
				else
				{
					_moveDirection += Vector3.Down * 2.5f;
					_moveDirection = _moveDirection.Normal * 10f;
				}*/
	}
}
