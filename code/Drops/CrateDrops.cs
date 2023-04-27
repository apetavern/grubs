namespace Grubs;

public static class CrateDrops
{
	private static readonly Dictionary<string, float> _dropChancesWeapons = new();
	private static readonly Dictionary<string, float> _dropChancesTools = new();

	private static readonly List<string> _dropMapWeapons = new();
	private static readonly List<float> _cumulativeDropPercentagesWeapons = new();

	private static readonly List<string> _dropMapTools = new();
	private static readonly List<float> _cumulativeDropPercentagesTools = new();
	private static bool _init;

	private static void Initialize()
	{
		if ( _init )
			return;

		foreach ( var prefab in Weapon.GetAllWeaponPrefabs() )
		{
			var weaponType = prefab.Root.GetValue<WeaponType>( nameof( WeaponType ) );
			var dropChance = prefab.Root.GetValue<float>( nameof( Weapon.DropChance ) );

			if ( weaponType is WeaponType.Weapon )
				_dropChancesWeapons.TryAdd( prefab.ResourcePath, dropChance );
			else if ( weaponType is WeaponType.Tool )
				_dropChancesTools.TryAdd( prefab.ResourcePath, dropChance );
		}

		InitDropMap( _dropChancesWeapons, _cumulativeDropPercentagesWeapons, _dropMapWeapons );
		InitDropMap( _dropChancesTools, _cumulativeDropPercentagesTools, _dropMapTools );

		_init = true;
	}

	private static void InitDropMap( Dictionary<string, float> dropChances, List<float> dropPercentages, List<string> dropMap )
	{
		var sumTotalOfDropRates = 0f;
		var numEntries = 0;

		foreach ( var (_, dropChance) in dropChances )
		{
			if ( dropChance <= 0 )
				continue;

			numEntries++;
			sumTotalOfDropRates += dropChance;
		}

		var i = 0;
		foreach ( var (resPath, dropChance) in dropChances )
		{
			if ( dropChance <= 0 )
				continue;

			dropPercentages.Add( dropChance / sumTotalOfDropRates );
			dropMap.Add( resPath );
			if ( i > 0 )
				dropPercentages[i] += dropPercentages[i - 1];
			i++;
		}
	}

	public static string GetRandomWeaponFromCrate()
	{
		if ( !_init )
			Initialize();

		return RollForItem( _cumulativeDropPercentagesWeapons, _dropMapWeapons );
	}

	public static string GetRandomToolFromCrate()
	{
		if ( !_init )
			Initialize();

		return RollForItem( _cumulativeDropPercentagesTools, _dropMapTools );
	}

	private static string RollForItem( List<float> dropPercentages, List<string> dropMap )
	{
		var roll = Game.Random.Float( 0f, 1f );

		var weapon = 0;
		while ( weapon < dropPercentages.Count && dropPercentages[weapon] <= roll )
			weapon++;

		return dropMap[weapon];
	}
}
