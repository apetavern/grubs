namespace Grubs;

[Prefab]
public partial class ShardedExplosiveGadgetComponent : ExplosiveGadgetComponent
{
	[Prefab, Net]
	public Prefab ShardGadgetPrefab { get; set; }

	[Prefab]
	public int AmountOfShards { get; set; }

	[Prefab]
	public int ShardCharge { get; set; } = 30;

	public override bool IsResolved()
	{
		return TimeUntilExplosion;
	}

	public override void Explode()
	{
		if ( !Game.IsServer )
			return;

		switch ( ExplosionReaction )
		{
			case ExplosiveReaction.Explosion:
				ExplosionHelper.Explode( Gadget.Position, Grub, ExplosionRadius, MaxExplosionDamage );
				break;
			case ExplosiveReaction.Incendiary:
				FireHelper.StartFiresAt( Gadget.Position, Gadget.Velocity.Normal, 10 );
				break;
		}

		for ( int i = 0; i < AmountOfShards; i++ )
		{
			var gadget = PrefabLibrary.Spawn<Gadget>( ShardGadgetPrefab );
			var directionRNG = new Random( Time.Tick + i );
			var direction = Rotation.LookAt( Vector3.Up + Vector3.Forward * directionRNG.Float( -1f, 1f ) * 0.5f, Vector3.Right );
			gadget.OnUse( Grub, Gadget.Position, direction, (int)MathF.Round( ShardCharge * directionRNG.Float( 0.5f, 1f ) ) );
		}

		ExplodeSoundClient( To.Everyone, ExplosionSound );
		Gadget.Delete();
	}
}
