namespace Grubs;

public static class FireHelper
{
	public static void StartFiresAt( Vector3 position, Vector3 moveDirection, int fireQuantity )
	{
		Game.AssertServer();

		for ( var i = 0; i < fireQuantity; i++ )
		{
			_ = new FireEntity( position + Vector3.Random.WithY( 0f ) * 30f, moveDirection + Vector3.Random.WithY( 0f ) * 30f );
		}
	}
}

[Category( "Weapons" )]
public class FireEntity : ModelEntity, IResolvable
{
	public bool Resolved => Time.Now > _expiryTime;

	private TimeSince _timeSinceLastTick { get; set; }
	private Vector3 _moveDirection { get; set; }
	private Vector3 _desiredPosition { get; set; }

	private readonly float _expiryTime;
	private const float FireTickRate = 0.15f;
	private const float fireSize = 20f;

	public FireEntity()
	{

	}

	public FireEntity( Vector3 startPosition, Vector3 moveDirection )
	{
		Position = startPosition + new Vector3().WithX( Game.Random.Int( 30 ) );
		_desiredPosition = Position;

		var tr = Trace.Ray( startPosition, startPosition + moveDirection )
			.WithAnyTags( "solid" )
			.Ignore( this )
			.Size( fireSize )
			.Run();

		if ( tr.Hit )
			_moveDirection = Vector3.Reflect( _moveDirection, tr.Normal );
		else
			_moveDirection = -moveDirection / 2f;

		_expiryTime = Time.Now + 3f;
		_timeSinceLastTick = Game.Random.Float( 0.25f );
	}

	public override void Spawn()
	{
		SetModel( "particles/flamemodel.vmdl" );
	}

	[GameEvent.Tick.Server]
	private void Tick()
	{
		Position = Vector3.Lerp( Position, _desiredPosition, Time.Delta * 10f );
		if ( Time.Now > _expiryTime )
			Delete();

		if ( _timeSinceLastTick < FireTickRate )
			return;

		Move();
		_timeSinceLastTick = 0f;
	}

	private void Move()
	{

		var midpoint = new Vector3( _desiredPosition.x, _desiredPosition.z );
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
		var tr = Trace.Sphere( fireSize * 1.5f, Position, _desiredPosition ).Run();
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
		}
	}
}
