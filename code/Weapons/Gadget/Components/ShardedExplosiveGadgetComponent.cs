namespace Grubs;

[Prefab]
public partial class ShardedExplosiveGadgetComponent : ExplosiveGadgetComponent
{
	[Prefab, Net]
	public Prefab ShardGadgetPrefab { get; set; }

	[Prefab, Net]
	public int AmountOfShards { get; set; }

	[Prefab, Net]
	public int ShardCharge { get; set; } = 30;

	[Prefab, Net]
	public int SpawnSpeed { get; set; } = 10;

	public override void Explode()
	{
		if ( !Game.IsServer )
			return;

		for ( int i = 0; i < AmountOfShards; i++ )
		{
			var newGadget = PrefabLibrary.Spawn<Gadget>( ShardGadgetPrefab );
			var randomDirection = new Random( Time.Tick + i );
			var direction = Rotation.LookAt( Vector3.Up + Vector3.Forward * randomDirection.Float( -1f, 1f ) * 0.5f, Vector3.Right );

			newGadget.Tags.Add( "shard" );
			newGadget.Owner = Grub;
			Grub.Player.Gadgets.Add( newGadget );

			newGadget.Position = Gadget.Position;
			newGadget.Velocity = direction.Forward * MathF.Round( ShardCharge * randomDirection.Float( 0.5f, 1f ) ) * SpawnSpeed;
		}

		ExplodeSoundClient( To.Everyone, ExplosionSound );
		Gadget.Delete();
	}
}
