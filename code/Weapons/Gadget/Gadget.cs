namespace Grubs;

[Prefab, Category( "Gadget" )]
public partial class Gadget : AnimatedEntity, IResolvable
{
	public Grub Grub => Owner as Grub;

	[Prefab]
	public bool UseModelCollision { get; set; } = false;

	[Prefab]
	public float CollisionRadius { get; set; } = 1.0f;

	[Prefab, Net]
	public bool ShouldCameraFollow { get; set; } = true;

	[Prefab, ResourceType( "vpcf" )]
	public string TrailParticle { get; set; }

	[Prefab, Net, ResourceType( "sound" )]
	public string StartSound { get; set; }
	private Sound _startSound;

	[Net]
	public Vector3 Target { get; set; } = Vector3.Zero;

	public bool Resolved => IsResolved();

	public override void Spawn()
	{
		Transmit = TransmitType.Always;
		EnableLagCompensation = true;
		Health = 1;

		if ( !string.IsNullOrEmpty( TrailParticle ) )
			Particles.Create( TrailParticle, this, "trail" );

		if ( UseModelCollision )
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		else
			SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Position, CollisionRadius );

		foreach ( var component in Components.GetAll<GadgetComponent>() )
		{
			component.Spawn();
		}
	}

	public override void ClientSpawn()
	{
		_startSound = this.SoundFromScreen( StartSound );

		foreach ( var component in Components.GetAll<GadgetComponent>() )
		{
			component.ClientSpawn();
		}
	}

	public bool IsResolved()
	{
		return Components.GetAll<GadgetComponent>().All( c => c.IsResolved() );
	}

	public void OnUse( Grub grub, Weapon weapon, int charge )
	{
		Owner = grub;
		grub.Player.Gadgets.Add( this );

		Position = weapon.GetStartPosition( true );

		Target = weapon.Target;

		if ( Target != Vector3.Zero )
		{
			SetMaterialGroup( 1 );
		}

		foreach ( var component in Components.GetAll<GadgetComponent>() )
		{
			component.OnUse( weapon, charge );
		}
	}

	public override void StartTouch( Entity other )
	{
		foreach ( var component in Components.GetAll<GadgetComponent>() )
		{
			component.OnTouch( other );
		}
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in Components.GetAll<GadgetComponent>() )
		{
			component.Simulate( client );
		}
	}

	protected override void OnDestroy()
	{
		OnClientDestroy();
	}

	[ClientRpc]
	private void OnClientDestroy()
	{
		_startSound.Stop();
	}

	[ClientRpc]
	public void PlayScreenSound( string sound )
	{
		this.SoundFromScreen( sound );
	}
}
