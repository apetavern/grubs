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
	private bool _hasLanded = false;

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

		var isGrounded = helper.TraceDirection( Vector3.Down ).Entity is not null;

		helper.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;

		var baseFriction = 2.0f;
		if ( isGrounded )
			baseFriction *= 12.0f;
		var parachuteAirFrictionModifier = !_hasLanded ? 1.5f : 5f;
		helper.ApplyFriction( baseFriction * parachuteAirFrictionModifier, Time.Delta );
		helper.TryMove( Time.Delta );

		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;

		_hasLanded = helper.TraceDirection( Vector3.Down ).Entity is not null;
		if ( _hasLanded )
		{
			Parachute?.SetAnimParameter( "landed", true );
			Gadget.Rotation = Angles.Zero.ToRotation();
		}
		else
		{
			Gadget.Rotation = Rotation.Lerp( Gadget.Rotation, new Angles( MathF.Sin( Time.Now * 2f ) * 15f, 0, 0 ).ToRotation(), Time.Delta );
		}
	}
}
