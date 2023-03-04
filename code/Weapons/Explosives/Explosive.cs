namespace Grubs;

[Prefab, Category( "Explosive" )]
public partial class Explosive : AnimatedEntity
{
	public Grub Grub => Owner as Grub;

	[Prefab]
	public float ExplosionRadius { get; set; } = 100.0f;

	[Prefab, Net]
	public float MaxExplosionDamage { get; set; } = 100f;

	[Prefab, Net]
	public float ExplosionForceMultiplier { get; set; } = 1.0f;

	/// <summary>
	/// The number of seconds before it explodes, set to "0" if something else handles the exploding.
	/// </summary>
	[Prefab]
	public float ExplodeAfter { get; set; } = 4.0f;

	[Prefab]
	public bool ShouldUseModelCollision { get; set; } = false;

	[Prefab]
	public float ExplosiveCollisionRadius { get; set; } = 1.0f;

	[Prefab, Net]
	public bool UseCustomPhysics { get; set; } = false;

	[Prefab]
	public bool ShouldRotate { get; set; } = true;

	[Prefab]
	public bool ShouldBounce { get; set; } = false;

	[Prefab, Net]
	public bool ShouldCameraFollow { get; set; } = true;

	[Prefab, ResourceType( "sound" )]
	public string ExplosionSound { get; set; }

	[Prefab]
	public ExplosiveReaction ExplosionReaction { get; set; }

	public override void Spawn()
	{
		Transmit = TransmitType.Always;
		EnableLagCompensation = true;
		Health = 1;

		if ( ShouldUseModelCollision )
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		else
			SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Position, ExplosiveCollisionRadius );
	}

	public void OnFired( Grub grub, Weapon weapon, int charge )
	{
		Owner = grub;
		// grub.Player.AddExplosive( this );

		foreach ( var component in Components.GetAll<ExplosiveComponent>() )
		{
			component.OnFired( weapon, charge );
		}
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in Components.GetAll<ExplosiveComponent>() )
		{
			component.Simulate( client );
		}

		if ( !UseCustomPhysics )
			HandlePhysicsTick();
	}

	private void HandlePhysicsTick()
	{
		// Apply gravity.
		Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 12f ).WithAnyTags( "player", "solid" ).WithoutTags( "dead" ).Ignore( this );
		helper.TryMove( Time.Delta );
		Velocity = helper.Velocity;
		Position = helper.Position;

		// TODO: What about bouncing and stuff?

		if ( ShouldRotate )
		{
			// Apply rotation using some shit I pulled out of my ass.
			var angularX = Velocity.x * 5f * Time.Delta;
			float degrees = angularX.Clamp( -20, 20 );
			Rotation = Rotation.RotateAroundAxis( new Vector3( 0, 1, 0 ), degrees );
		}
		else
		{
			Rotation = Rotation.Identity;
		}
	}
}
