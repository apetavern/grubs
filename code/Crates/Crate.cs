namespace Grubs;

[Prefab, Category( "Crate" )]
public partial class Crate : ModelEntity
{
	private AnimatedEntity _parachute;
	private static readonly BBox Bbox = new( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 16 ) );

	public override void Spawn()
	{
		base.Spawn();

		Health = 25;

		_ = SpawnParachuteDelayed();
	}

	private async Task SpawnParachuteDelayed()
	{
		await Task.DelaySeconds( 1 );
		_parachute = new AnimatedEntity( "models/crates/crate_parachute/crate_parachute.vmdl", this );
	}

	[Event.Tick.Server]
	private void Tick()
	{
		// If this crate is parented, don't move it.
		if ( Parent is not null )
			return;

		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace
			.Size( Bbox )
			.Ignore( this )
			.WorldAndEntities();
		GroundEntity = mover.TraceDirection( Vector3.Down ).Entity;

		if ( GroundEntity == null )
			mover.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		else
		{
			_parachute?.DeleteAsync( 0.3f );
			_parachute?.SetAnimParameter( "landed", true );
			_parachute = null;
			mover.Velocity = 0;
		}

		const float airResistance = 2.0f;
		mover.ApplyFriction( airResistance * (_parachute is not null ? 1.5f : 0.5f), Time.Delta );
		mover.TryMove( Time.Delta );

		Position = mover.Position;
		// Add swap to velocity to make the rotation sway look less weird
		Velocity = mover.Velocity + new Vector3( _parachute is not null ? MathF.Sin( Time.Now * 2f ) * 4f : 0f, 0, 0 );
		// Rotation to add some sway, slerped to smooth it
		Rotation = Rotation.Slerp( Rotation, Rotation.Identity * new Angles( _parachute is not null ? MathF.Sin( Time.Now * 2f ) * 15f : 0f, 0, 0 ).ToRotation(), 0.75f );
	}

	// TODO: Why do we manually create a bbox?
	private void OnTouch()
	{
		foreach ( var component in Components.GetAll<PickupComponent>() )
		{
			component.OnPickup( null );
		}

		Delete();
	}

	// Maybe paramter based?
	[ConCmd.Admin( "gr_spawn_crate" )]
	private static void SpawnCrate()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( PrefabLibrary.TrySpawn<Crate>( "crates/healthcrate.prefab", out var crate ) )
		{
			crate.Position = player.ActiveGrub.Position + Vector3.Up * 2000;
		}
	}
}
