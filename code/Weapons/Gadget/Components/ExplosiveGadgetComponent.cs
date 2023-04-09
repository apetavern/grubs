namespace Grubs;

[Prefab]
public partial class ExplosiveGadgetComponent : GadgetComponent
{
	[Prefab]
	public float ExplosionRadius { get; set; } = 100.0f;

	[Prefab, Net]
	public float MaxExplosionDamage { get; set; } = 100f;

	[Prefab, Net]
	public float ExplosionForceMultiplier { get; set; } = 1.0f;

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

	public override void OnClientSpawn()
	{
		if ( ExplodeAfter > 0 )
			_ = new ExplosiveGadgetWorldPanel( Gadget );
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

	protected virtual void Explode()
	{
		if ( !Game.IsServer )
			return;

		switch ( ExplosionReaction )
		{
			case ExplosiveReaction.Explosion:
				ExplosionHelper.Explode( Gadget.Position, Grub, ExplosionRadius, MaxExplosionDamage );
				break;
			case ExplosiveReaction.Incendiary:
				// FireHelper.StartFiresAt( Position, Segments[Segments.Count - 1].EndPos - Segments[Segments.Count - 1].StartPos, 10 );
				break;
		}

		Gadget.PlaySound( ExplosionSound );
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
