namespace Grubs.Drops;

[Title( "Grubs - Crate Utility" ), Category( "Utility" )]
public sealed class CrateUtility : Component
{
	[Property] public GameObject WeaponsCratePrefab { get; set; }
	[Property] public GameObject HealthCratePrefab { get; set; }
	
	public static CrateUtility Instance { get; set; }

	public CrateUtility()
	{
		Instance = this;
	}

	public GameObject SpawnCrate( DropType dropType )
	{
		var go = dropType switch
		{
			DropType.Weapon => WeaponsCratePrefab.Clone(),
			DropType.Health => HealthCratePrefab.Clone(),
			_ => WeaponsCratePrefab.Clone()
		};

		go.NetworkSpawn();
		
		return go;
	}
}
