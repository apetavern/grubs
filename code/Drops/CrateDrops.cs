namespace Grubs;

public static class CrateDrops
{
	public static Dictionary<string, float> DropChancesWeapons = new();
	public static Dictionary<string, float> DropChancesTools = new();
	public static bool DropChancesPopulated = false;

	static List<string> _dropMapWeapons = new();
	static List<float> _cumulativeDropPercentagesWeapons = new();

	static List<string> _dropMapTools = new();
	static List<float> _cumulativeDropPercentagesTools = new();
	static bool _init;

	static void Initialize()
	{
		if ( _init )
			return;

		InitDropMap( DropChancesWeapons, _cumulativeDropPercentagesWeapons, _dropMapWeapons );
		InitDropMap( DropChancesTools, _cumulativeDropPercentagesTools, _dropMapTools );

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
		while ( dropPercentages[weapon] <= roll )
			weapon++;

		return dropMap[weapon];
	}
}
