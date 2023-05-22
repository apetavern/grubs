namespace Grubs;

[Prefab]
public partial class ParachuteGadgetComponent : GadgetComponent
{
	[Prefab, ResourceType( "sound" )]
	public string SpawnSound { get; set; }

	[Net]
	private AnimatedEntity Parachute { get; set; }

	private readonly Material _spawnMaterial = Material.Load( "materials/effects/teleport/teleport.vmat" );
	private TimeSince _timeSinceSpawned;
	private bool _wasGrounded = false;

	public override void Spawn()
	{
		Gadget.SetMaterialOverride( _spawnMaterial );
		Gadget.PlaySound( SpawnSound );

		_timeSinceSpawned = 0f;

		_ = SpawnParachuteDelayed();
	}

	private async Task SpawnParachuteDelayed()
	{
		await GameTask.DelaySeconds( 0.2f );
		Parachute = new AnimatedEntity( "models/crates/crate_parachute/crate_parachute.vmdl", Entity );
	}

	public override void Simulate( IClient client )
	{
		if ( Game.IsServer && _timeSinceSpawned > 1f )
			Gadget.ClearMaterialOverride();

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Grub )
			.Ignore( Gadget )
			.WithAnyTags( Tag.Player, Tag.Solid, Tag.Gadget )
			.WithoutTags( Tag.Dead );

		_wasGrounded |= helper.TraceDirection( Vector3.Down ).Entity is not null;

		if ( !_wasGrounded )
			helper.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		else
			Parachute.SetAnimParameter( "landed", true );

		var parachuteAirFrictionModifier = Parachute is not null ? 1.5f : 0.5f;
		helper.ApplyFriction( 2.0f * parachuteAirFrictionModifier, Time.Delta );
		helper.TryMove( Time.Delta );

		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;

		if ( !_wasGrounded )
			Gadget.Rotation = Rotation.Lerp( Gadget.Rotation, new Angles( MathF.Sin( Time.Now * 2f ) * 15f, 0, 0 ).ToRotation(), Time.Delta );
		else
			Gadget.Rotation = Angles.Zero.ToRotation();
	}
}
