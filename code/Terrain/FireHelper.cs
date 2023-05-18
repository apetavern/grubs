namespace Grubs;

public static class FireHelper
{
	public static void StartFiresAt( Vector3 position, Vector3 moveDirection, int fireQuantity )
	{
		Game.AssertServer();

		for ( var i = 0; i < fireQuantity; i++ )
		{
			var baseDirection = Vector3.Random.WithY( 0f ) * 5f;
			_ = new FireEntity(
				position + Vector3.Random.WithY( 0f ) * 30f,
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
		Log.Info( _moveDirection );

		_expiryTime = Time.Now + Game.Random.Float( 0.5f, 2.5f );
	}

	public override void Spawn()
	{
		Model = Model.Load( "particles/flamemodel.vmdl" );
		// FireParticle = Particles.Create( "particles/fire/fire_base.vpcf", this, true );
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
		Velocity += GamemodeSystem.Instance.ActiveWindForce * 512f * Time.Delta;

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
			if ( collisionTrace.Entity is Grub grub )
			{
				grub.TakeDamage(
					DamageInfoExtension.FromExplosion( 0.25f, Position, Vector3.Up * 32f, this ) );
				grub.ApplyAbsoluteImpulse( (grub.Position - Position).Normal * 32f );
			}
			else if ( collisionTrace.Entity is Gadget gadget )
			{
				if ( gadget.Name == "Weapons Crate" || gadget.Name == "Tools Crate" || gadget.Name == "Health Crate" )
				{
					gadget.TakeDamage(
						DamageInfoExtension.FromExplosion( 0.25f, Position, Vector3.Up * 32f, this ) );
				}
			}
		}

		var terrain = GrubsGame.Instance.Terrain;
		var materials = terrain.GetActiveMaterials( MaterialsConfig.Destruction );
		terrain.SubtractCircle( new Vector2( Position.x, Position.z ), fireSize, materials );

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
