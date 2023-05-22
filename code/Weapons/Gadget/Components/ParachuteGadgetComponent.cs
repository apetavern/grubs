namespace Grubs;

[Prefab]
public partial class ParachuteGadgetComponent : GadgetComponent
{
	[Prefab, ResourceType( "sound" )]
	public string SpawnSound { get; set; }

	private readonly Material _spawnMaterial = Material.Load( "materials/effects/teleport/teleport.vmat" );
	private AnimatedEntity _parachute;
	private TimeSince TimeSinceSpawned { get; set; } = 0f;

	public override void Spawn()
	{
		Gadget.SetMaterialOverride( _spawnMaterial );
		Gadget.PlaySound( SpawnSound );

		_ = SpawnParachuteDelayed();
	}

	private async Task SpawnParachuteDelayed()
	{
		await GameTask.DelaySeconds( 0.2f );
		_parachute = new AnimatedEntity( "models/crates/crate_parachute/crate_parachute.vmdl", Entity );
	}

	public override void Simulate( IClient client )
	{
		if ( Game.IsServer )
		{
			if ( TimeSinceSpawned > 1f )
				Gadget.ClearMaterialOverride();
		}

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Grub )
			.Ignore( Gadget )
			.WithAnyTags( Tag.Player, Tag.Solid, Tag.Gadget )
			.WithoutTags( Tag.Dead );

		var groundEntity = helper.TraceDirection( Vector3.Down ).Entity;

		if ( groundEntity is null )
		{
			helper.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		}
		else
		{
			_parachute?.DeleteAsync( 0.3f );
			_parachute?.SetAnimParameter( "landed", true );
			_parachute = null;

			helper.Velocity = 0;
		}

		var parachuteAirFrictionModifier = _parachute is not null ? 1.5f : 0.5f;
		helper.ApplyFriction( 2.0f * parachuteAirFrictionModifier, Time.Delta );
		helper.TryMove( Time.Delta );

		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;
		Gadget.Rotation = Rotation.Lerp(
			Gadget.Rotation,
			new Angles( _parachute is not null ? MathF.Sin( Time.Now * 2f ) * 15f : 0f, 0, 0 ).ToRotation(),
			Time.Delta
		);
	}
}
