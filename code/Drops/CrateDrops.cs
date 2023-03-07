using Sandbox;

namespace Grubs;

public static class CrateDrops
{
	public static Dictionary<string, float> DropChances = new();
	public static bool DropChancesPopulated = false;
	static string[] _dropMap;
	static float[] _cumulativeDropPercentages;
	static bool _init;

	static void Initialize()
	{
		if ( _init )
			return;

		var sumTotalOfDropRates = 0f;
		var numEntries = 0;
		foreach ( var (_, dropChance) in DropChances )
		{
			if ( dropChance <= 0 )
				continue;

			numEntries++;
			sumTotalOfDropRates += dropChance;
		}

		_cumulativeDropPercentages = new float[numEntries];
		_dropMap = new string[numEntries];
		var i = 0;
		foreach ( var (resPath, dropChance) in DropChances )
		{
			if ( dropChance <= 0 )
				continue;

			_cumulativeDropPercentages[i] = dropChance / sumTotalOfDropRates;
			_dropMap[i] = resPath;
			if ( i > 0 )
				_cumulativeDropPercentages[i] += _cumulativeDropPercentages[i - 1];
			i++;
		}

		_init = true;
	}

	public static string GetRandomWeaponFromCrate()
	{
		if ( !_init )
			Initialize();

		var roll = Game.Random.Float( 0f, 1f );

		var weapon = 0;
		while ( _cumulativeDropPercentages[weapon] <= roll )
			weapon++;

		return _dropMap[weapon];
	}
}
