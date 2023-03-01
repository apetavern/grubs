namespace Grubs;

[Prefab, Category( "Explosive" )]
public partial class Explosive : AnimatedEntity
{
	public Grub Grub => Owner as Grub;

	[Prefab]
	public float ExplosionRadius { get; set; } = 100.0f;

	[Prefab, Net]
	public float ExplosionDamage { get; set; } = 50f;

	[Prefab]
	public float ExplosionForceMultiplier { get; set; } = 1.0f;

	/// <summary>
	/// The number of seconds before it explodes, set to "-1" if something else handles the exploding.
	/// </summary>
	[Prefab]
	public float ExplodeAfter { get; set; } = 4.0f;

	[Prefab]
	public bool ShouldUseModelCollision { get; set; } = false;

	[Prefab]
	public float ExplosiveCollisionRadius { get; set; } = 1.0f;

	[Prefab]
	public bool ShouldRotate { get; set; } = true;

	[Prefab]
	public bool ShouldBounce { get; set; } = false;

	[Prefab]
	public bool ShouldCameraFollow { get; set; } = true;

	[Prefab, ResourceType( "sound" )]
	public string ExplosionSound { get; set; }

	[Prefab]
	private ExplosiveReaction ExplosionReaction { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		// TODO: Figure out something else.
		GamemodeSystem.Instance.Explosives.Add( this );

		Transmit = TransmitType.Always;
		Health = 1;
	}

	public override void Simulate( IClient client )
	{
		foreach ( var component in Components.GetAll<ExplosiveComponent>() )
		{
			component.Simulate( client );
		}
	}
}

/// <summary>
/// Defines the type of reaction a <see cref="Explosion"/> has when it explodes.
/// </summary>
public enum ExplosiveReaction
{
	/// <summary>
	/// Produces a regular explosion.
	/// </summary>
	Explosion,
	/// <summary>
	/// Produces a fire.
	/// </summary>
	Incendiary
}
