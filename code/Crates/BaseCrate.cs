using Grubs.Player;
using Grubs.States;
using Grubs.Terrain.Shapes;
using Grubs.Utils;

namespace Grubs.Crates;

/// <summary>
/// The base class for all crates.
/// </summary>
[Category( "Crates" )]
public partial class BaseCrate : ModelEntity, IResolvable
{
	public bool Resolved => Velocity.IsNearlyZero( 2.5f );

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

		SpawnParachuteDelayed();
	}

	public async void SpawnParachuteDelayed()
	{
		await Task.DelaySeconds( 1f );
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
		PickupZone.Position = Position;
	}
}
