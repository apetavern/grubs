namespace Grubs.Equipment.Gadgets.Ground;

[Title( "Grubs - Landmine Utility" ), Category( "Utility" )]
public class LandmineUtility : Component
{
	[Property] public GameObject LandminePrefab { get; set; }
	[Property] public GameObject OildrumPrefab { get; set; }
	public static LandmineUtility Instance { get; set; }

	public LandmineUtility()
	{
		Instance = this;
	}

	public GameObject Spawn( Vector3 position )
	{
		var go = Game.Random.Float() > 0.7f ? OildrumPrefab.Clone() : LandminePrefab.Clone();
		go.WorldPosition = position.WithY( 512f );
		go.Tags.Add( "drop" );
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );
		go.NetworkSpawn();

		return go;
	}
}
