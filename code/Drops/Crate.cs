namespace Grubs.Drops;

[Title( "Grubs - Crate" ), Category( "Grubs" )]
public sealed class Crate : Component, Component.ITriggerListener
{
	[Property] public DropType DropType { get; set; } = DropType.Weapon;

	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Tags.Has( "player" ) )
		{
			Log.Info( CrateDrops.GetRandomWeaponFromCrate() );
		}
	}

	protected override void OnUpdate()
	{
	}
}
