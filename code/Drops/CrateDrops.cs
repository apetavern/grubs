using Grubs.Equipment;
using Sandbox.Diagnostics;

namespace Grubs.Drops;

public static class CrateDrops
{
	private static readonly Logger Log = new("CrateDrops");

	private static readonly Dictionary<string, float> _dropChancesWeapons = new();
	private static readonly Dictionary<string, float> _dropChancesTools = new();

	private static readonly List<string> _dropMapWeapons = new();
	private static readonly List<float> _cumulativeDropPercentagesWeapons = new();

	private static readonly List<string> _dropMapTools = new();
	private static readonly List<float> _cumulativeDropPercentagesTools = new();
	private static bool _init = false;

	private static void Initialize()
	{
		if ( _init )
			return;

		foreach ( var res in EquipmentResource.All )
		{
			var equipmentType = res.Type;
			var dropChance = res.DropChance;

			if ( equipmentType is EquipmentType.Weapon )
				_dropChancesWeapons.TryAdd( res.ResourcePath, dropChance );
			else if ( equipmentType is EquipmentType.Tool )
				_dropChancesTools.TryAdd( res.ResourcePath, dropChance );
		}

		InitDropMap( _dropChancesWeapons, _cumulativeDropPercentagesWeapons, _dropMapWeapons );
		InitDropMap( _dropChancesTools, _cumulativeDropPercentagesTools, _dropMapTools );

		if ( !Game.IsEditor )
			_init = true;
	}

	private static void InitDropMap( Dictionary<string, float> dropChances, List<float> dropPercentages,
		List<string> dropMap )
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
		{
			weapon++;
		}

		return dropMap[weapon];
	}

	[ConCmd( "gr_show_droprates" )]
	public static void ShowDropRates()
	{
		// Ensure the drop tables have been initialized.
		// GetRandomToolFromCrate();
		GetRandomWeaponFromCrate();

		// Print weapon drop table.
		Log.Info( "=== WEAPONS DROP TABLE ===" );
		for ( var i = 0; i < _cumulativeDropPercentagesWeapons.Count; i++ )
		{
			float dropChance;
			if ( i > 0 )
				dropChance = _cumulativeDropPercentagesWeapons[i] - _cumulativeDropPercentagesWeapons[i - 1];
			else
				dropChance = _cumulativeDropPercentagesWeapons[i];

			Log.Info( $"{_dropMapWeapons[i]}: {dropChance * 100f}%" );
		}

		Log.Info( "=== TOOLS DROP TABLE ===" );
		Log.Info( "No tools available yet." );
		// Print tool drop table.
		// for ( var i = 0; i < _cumulativeDropPercentagesTools.Count; i++ )
		// {
		// 	float dropChance;
		// 	if ( i > 0 )
		// 		dropChance = _cumulativeDropPercentagesTools[i] - _cumulativeDropPercentagesTools[i - 1];
		// 	else
		// 		dropChance = _cumulativeDropPercentagesTools[i];
		//
		// 	Log.Info( $"{_dropMapTools[i]}: {dropChance * 100f}%" );
		// }
	}
}
