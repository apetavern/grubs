using Grubs.Player;
using Grubs.States;
using Grubs.Utils.Extensions;

namespace Grubs.Utils;

/// <summary>
/// A utility class to use fire.
/// </summary>
public sealed class FireEntity : ModelEntity, IResolvable
{
	public bool Resolved => Time.Now > _expiryTime;

	private TimeSince TimeSinceLastTick { get; set; }
	private Vector3 MoveDirection { get; set; }

	private readonly float _expiryTime;
	private const float FireTickRate = 0.25f;

	public FireEntity()
	{
	}

	public FireEntity( Vector3 startPos, Vector3 movementDirection )
	{
		Position = startPos + new Vector3().WithX( Rand.Int( 30 ) );
		MoveDirection = movementDirection;
		_expiryTime = Time.Now + 2.5f;
		TimeSinceLastTick = Rand.Float( 0.25f );
	}

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "particles/flamemodel.vmdl" );
	}

	[Sandbox.Event.Tick.Server]
	private void Tick()
	{
		if ( Time.Now > _expiryTime )
			Delete();

		if ( TimeSinceLastTick < FireTickRate )
			return;

		Move();
		TimeSinceLastTick = 0f;
	}

	private void Move()
	{
		const float fireSize = 20f;

		var midpoint = new Vector3( Position.x, Position.z );

		var didDamage = GrubsGame.Current.TerrainMap.DestructSphere( midpoint, fireSize );
		GrubsGame.ExplodeClient( To.Everyone, midpoint, fireSize );

		var sourcePos = Position;
		foreach ( var grub in All.OfType<Grub>().Where( x => Vector3.DistanceBetween( sourcePos, x.Position ) <= fireSize ) )
		{
			if ( !grub.IsValid() || grub.LifeState != LifeState.Alive )
				continue;

			var dist = Vector3.DistanceBetween( Position, grub.Position );
			if ( dist > fireSize )
				continue;

			var distanceFactor = 1.0f - Math.Clamp( dist / fireSize, 0, 1 );
			var force = distanceFactor * 1000; // TODO: PhysicsGroup/Body is invalid on grubs

			var dir = (grub.Position - Position).Normal;
			grub.ApplyAbsoluteImpulse( dir * force );

			grub.TakeDamage( DamageInfoExtension.FromExplosion( 6, Position, Vector3.Up * 32, this ) );
		}

		Position += MoveDirection;
		MoveDirection += Vector3.Down * 2.5f;
		MoveDirection = MoveDirection.Normal * 10f;

		if ( didDamage )
			GrubsGame.Current.RegenerateMap();
	}
}
