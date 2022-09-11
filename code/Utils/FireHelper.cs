using Grubs.Player;
using Grubs.Utils.Extensions;
using System.Threading.Tasks;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Grubs.States;

namespace Grubs.Utils;

/// <summary>
/// A utility class to use fire.
/// </summary>
public partial class Fire : ModelEntity, IResolvable
{
	private float FireTickRate { get; set; } = 0.25f;
	private TimeSince TimeSinceLastTick { get; set; }
	private float ExpiryTime { get; set; }
	public bool Resolved { get; set; }
	public Vector3 MoveDirection { get; set; }

	//Particles system { get; set; }

	public Fire() { }

	public Fire( Vector3 startPos, Vector3 movementDirection )
	{
		ExpiryTime = Time.Now + 2.5f;
		TimeSinceLastTick = Rand.Float( 0.25f );
		Position = startPos + new Vector3().WithX( Rand.Int( 30 ) );
		MoveDirection = movementDirection;
		//SetFireParticle( To.Everyone, this );
	}

	public override void Spawn()
	{
		base.Spawn();
		ReceiveFireParticle();
	}

	private void Move()
	{
		float fireSize = 20f;

		var midpoint = new Vector3( Position.x, Position.z );

		bool DidDamage = GrubsGame.Current.TerrainMap.DestructSphere( midpoint, fireSize );
		GrubsGame.ExplodeClient( To.Everyone, midpoint, fireSize );

		var sourcePos = Position;
		foreach ( var grub in Entity.All.OfType<Grub>().Where( x => Vector3.DistanceBetween( sourcePos, x.Position ) <= fireSize ) )
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

		if ( DidDamage )
		{
			GrubsGame.Current.RegenerateMap();
		}
	}

	public void Tick()
	{
		if ( Time.Now > ExpiryTime )
		{
			Resolved = true;
			Delete();
		}

		if ( TimeSinceLastTick > FireTickRate )
		{
			Move();
			TimeSinceLastTick = 0f;
		}
	}

	public void ReceiveFireParticle()
	{
		SetModel( "particles/flamemodel.vmdl" );
		//system = Particles.Create( "particles/fire_loop.vpcf", this, "fire", true );//doesn't do shit
	}
}

public partial class FireHelper : Entity
{
	static FireHelper Instance { get; set; }
	public List<Fire> FireInstances { get; set; } = new();

	public static void StartFiresAt( Vector3 origin, Vector3 moveDirection, int qty )
	{
		Host.AssertServer();

		if ( !Host.IsServer )
			return;

		if ( Instance is null || !Instance.IsValid )
			Instance = new();

		Instance.Position = origin;

		// Create instances of fire, delay their initial ticks and set their death time.
		for ( int i = 0; i < qty; i++ )
		{
			var fire = new Fire( Instance.Position + Vector3.Random.WithY( 0 ) * 30, moveDirection + Vector3.Random.WithY( 0 ) * 30 );

			Instance.FireInstances.Add( fire );
		}
	}

	[Sandbox.Event.Tick.Server]
	public void TickFire()
	{
		if ( !FireInstances.Any() )
			return;

		foreach ( var fire in FireInstances )
		{
			if ( fire.IsValid() )
				fire.Tick();
		}
	}
}
