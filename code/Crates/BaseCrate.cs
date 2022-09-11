using Grubs.Player;
using Grubs.Terrain.Shapes;
using Grubs.Utils;

namespace Grubs.Crates;

/// <summary>
/// The base class for all crates.
/// </summary>
public partial class BaseCrate : ModelEntity
{
	/// <summary>
	/// The zone that will check for Grubs picking the crate up.
	/// </summary>
	[Net]
	protected PickupZone PickupZone { get; set; } = null!;

	private static readonly BBox Bbox = new( new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 16 ) );
	private AnimatedEntity? _parachute;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/editor/proxy_helper.vmdl" );

		Health = 25;

		PickupZone = new PickupZone()
			.WithPosition( Position )
			.WithShape( BoxShape.WithSize( new Vector3( 32, 32, 32 ) ).WithOffset( new Vector3( -16, -16, -16 ) ) )
			.Finish<PickupZone>();
		PickupZone.SetParent( this );

		_parachute = new AnimatedEntity( "models/crates/crate_parachute/crate_parachute.vmdl", this );
	}

	/// <summary>
	/// Called when a Grub is trying to pickup the crate.
	/// </summary>
	/// <param name="grub">The Grub that is trying to pickup the crate.</param>
	public virtual void OnPickup( Grub grub )
	{
		Delete();
		PickupZone.Delete();
	}

	[Event.Tick.Server]
	private void Tick()
	{
		Move();

		if ( Health > 0 )
			return;

		// TODO: A Grub should've interacted with this to blow it up in some way.
		ExplosionHelper.Explode( Position, null! );
		Delete();
	}

	private void Move()
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
			mover.Velocity += Map.Physics.Gravity * Time.Delta;
		else
		{
			_parachute?.Delete();
			_parachute = null;
			mover.Velocity = 0;
		}

		const float airResistance = 2.0f;
		mover.ApplyFriction( airResistance, Time.Delta );
		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
		PickupZone.Position = Position;
	}
}
