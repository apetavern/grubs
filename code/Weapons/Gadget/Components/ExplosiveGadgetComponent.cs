namespace Grubs;

[Prefab]
public partial class ExplosiveGadgetComponent : GadgetComponent
{
	[Prefab]
	public float DestructionRadius { get; set; } = 100.0f;

	[Prefab]
	public float DamageRadius { get; set; } = 125.0f;

	[Prefab, Net]
	public float MaxExplosionDamage { get; set; } = 100f;

	[Prefab, Net]
	public bool ExplodeOnTouch { get; set; } = false;

	[Prefab, Net]
	public bool DeleteOnExplode { get; set; } = true;

	/// <summary>
	/// The number of seconds before it explodes, set to "0" if something else handles the exploding.
	/// </summary>
	[Prefab, Net]
	public float ExplodeAfter { get; set; } = 4.0f;

	[Prefab, ResourceType( "sound" )]
	public string ExplosionSound { get; set; }

	[Prefab]
	public ExplosiveReaction ExplosionReaction { get; set; }

	[Net]
	public TimeUntil TimeUntilExplosion { get; private set; }

	public override void ClientSpawn()
	{
		if ( ExplodeAfter > 0 )
			_ = new UI.ExplosiveGadgetWorldPanel( Gadget );
	}

	public override void OnUse( Weapon weapon, int charge )
	{
		if ( ExplodeAfter > 0 )
		{
			TimeUntilExplosion = ExplodeAfter;
			ExplodeAfterSeconds( ExplodeAfter );
		}
	}

	public override bool IsResolved()
	{
		return TimeUntilExplosion;
	}

	public async void ExplodeAfterSeconds( float seconds )
	{
		await GameTask.DelaySeconds( seconds );

		if ( !Gadget.IsValid() )
			return;

		Explode();
	}

	public virtual void Explode()
	{
		if ( !Game.IsServer )
			return;

		if ( DeleteOnExplode )
			Gadget.QueuedForDeletion = true;

		switch ( ExplosionReaction )
		{
			case ExplosiveReaction.Explosion:
				ExplosionHelper.Explode( Gadget.Position, Grub, DestructionRadius, DamageRadius, MaxExplosionDamage );
				break;
			case ExplosiveReaction.Incendiary:
				FireHelper.StartFiresAt( Gadget.Position, Gadget.Velocity.Normal * 10f, 10 );
				break;
		}

		// Play it from world since the gadget deletes and the sound will move to (0, 0, 0).
		Sound.FromWorld( To.Everyone, ExplosionSound, Gadget.Position );

		if ( Gadget.QueuedForDeletion )
			Gadget.Delete();
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
