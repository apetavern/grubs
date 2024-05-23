namespace Grubs.Equipment.Gadgets.Ground;

[Title( "Grubs - Landmine Utility" ), Category( "Utility" )]
public class LandmineUtility : Component
{
	[Property] public GameObject LandminePrefab { get; set; }

	public static LandmineUtility Instance { get; set; }

	public LandmineUtility()
	{
		Instance = this;
	}

	public GameObject Spawn( Vector3 position )
	{
		var go = LandminePrefab.Clone();
		go.Transform.Position = position.WithY( 512f );
		go.NetworkSpawn();

		return go;
	}
}
