using Grubs.Weapons.Base;

namespace Grubs.Utils;

/// <summary>
/// A utility class to handle drop chance of weapons.
/// </summary>
public static class CrateDropTables
{
	private static bool _init;
	private static float[] _cumulativeDropPercentages = null!;
	private static WeaponAsset[] _dropMap = null!;

	private static void Init()
	{
		if ( _init )
			return;

		var sumTotalOfDropRates = 0f;
		var numEntries = 0;
		foreach ( var weaponAsset in WeaponAsset.All )
		{
			if ( weaponAsset.DropChance <= 0 )
				continue;

			numEntries++;
			sumTotalOfDropRates += weaponAsset.DropChance;
		}

		// Calculate the cumulative drop percentage of each Weapon in the crate.
		_cumulativeDropPercentages = new float[numEntries];
		_dropMap = new WeaponAsset[numEntries];
		var i = 0;
		foreach ( var weaponAsset in WeaponAsset.All )
		{
			if ( weaponAsset.DropChance <= 0 )
				continue;

			_cumulativeDropPercentages[i] = weaponAsset.DropChance / sumTotalOfDropRates;
			_dropMap[i] = weaponAsset;
			if ( i > 0 )
				_cumulativeDropPercentages[i] += _cumulativeDropPercentages[i - 1];

			i++;
		}

		_init = true;
	}

	/// <summary>
	/// Gets a random <see cref="WeaponAsset"/> for a weapon crate.
	/// </summary>
	/// <returns>The randomly chosen weapon.</returns>
	public static WeaponAsset GetRandomWeaponFromCrate()
	{
		if ( !_init )
			Init();

		var random = new Random( Time.Now.CeilToInt() );
		var roll = random.Next( 100 ) / 100f;

		var weapon = 0;
		while ( _cumulativeDropPercentages[weapon] <= roll )
			weapon++;

		return _dropMap[weapon];
	}
}
