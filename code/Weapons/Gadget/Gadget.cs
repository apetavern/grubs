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

	[Prefab, Net]
	public bool ExplodeOnKilled { get; set; } = false;

	[Prefab, ResourceType( "vpcf" )]
	public string TrailParticle { get; set; }

	[Prefab, Net, ResourceType( "sound" )]
	public string StartSound { get; set; }
	private Sound _startSound;

	public bool Resolved => IsResolved();

	public bool IsCrate => Components.Get<CrateGadgetComponent>() is not null;

	private IEnumerable<GadgetComponent> _sortCache;

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

		foreach ( var component in GetSortedComponents() )
		{
			component.Spawn();
		}
	}

	public override void ClientSpawn()
	{
		_startSound = this.SoundFromScreen( StartSound );

		foreach ( var component in GetSortedComponents() )
		{
			component.ClientSpawn();
		}
	}

	public bool IsResolved()
	{
		return GetSortedComponents().All( c => c.IsResolved() );
	}

	public void OnUse( Grub grub, Weapon weapon, int charge )
	{
		Owner = grub;
		grub.Player.Gadgets.Add( this );

		Position = weapon.GetStartPosition( true );

		foreach ( var component in GetSortedComponents() )
		{
			component.OnUse( weapon, charge );
		}
	}

	public override void Touch( Entity other )
	{
		foreach ( var component in GetSortedComponents() )
		{
			component.Touch( other );
		}
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in GetSortedComponents() )
		{
			component.Simulate( client );
		}
	}

	public override void TakeDamage( DamageInfo damageInfo )
	{
		base.TakeDamage( damageInfo );

		if ( Health > 0 )
			return;

		if ( !ExplodeOnKilled || damageInfo.HasTag( Tag.OutOfArea ) )
			return;

		ExplosionHelper.Explode( Position, this, 50f );
		FireHelper.StartFiresAt( Position, Vector3.Random.WithY( 0f ) * 30f, 4 );

		PlayScreenSound( "explosion_short_tail" );
		Delete();
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

	private IEnumerable<GadgetComponent> GetSortedComponents()
	{
		_sortCache ??= Components.GetAll<GadgetComponent>().OrderByDescending( c => c.SortOrder );
		return _sortCache;
	}
}
