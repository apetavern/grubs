using Grubs.Equipment;
using Sandbox.Diagnostics;

namespace Grubs.Drops;

public static class CrateDrops
{
	private static readonly Logger Log = new( "CrateDrops" );

	private static readonly Dictionary<string, float> WeaponDropChances = new();
	private static readonly Dictionary<string, float> ToolDropChances = new();

	private static readonly List<string> WeaponDropMap = new();
	private static readonly List<float> CumulativeWeaponDropPercentages = new();

	private static readonly List<string> ToolDropMap = new();
	private static readonly List<float> CumulativeToolDropPercentages = new();
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
				WeaponDropChances.TryAdd( res.ResourcePath, dropChance );
			else if ( equipmentType is EquipmentType.Tool )
				ToolDropChances.TryAdd( res.ResourcePath, dropChance );
		}

		InitDropMap( WeaponDropChances, CumulativeWeaponDropPercentages, WeaponDropMap );
		InitDropMap( ToolDropChances, CumulativeToolDropPercentages, ToolDropMap );

		_init = true;
	}

	private static void InitDropMap( Dictionary<string, float> dropChances, List<float> dropPercentages,
		List<string> dropMap )
	{
		var sumTotalOfDropRates = 0f;

		foreach ( var (_, dropChance) in dropChances )
		{
			if ( dropChance <= 0 )
				continue;

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

		return RollForItem( CumulativeWeaponDropPercentages, WeaponDropMap );
	}

	public static string GetRandomToolFromCrate()
	{
		if ( !_init )
			Initialize();

		return RollForItem( CumulativeToolDropPercentages, ToolDropMap );
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
		GetRandomToolFromCrate();
		GetRandomWeaponFromCrate();

		Log.Info( "Weapons Available: " + CumulativeWeaponDropPercentages.Count );
		Log.Info( "Tools Available: " + CumulativeToolDropPercentages.Count );

		// Print weapon drop table.
		Log.Info( "=== WEAPONS DROP TABLE ===" );
		for ( var i = 0; i < CumulativeWeaponDropPercentages.Count; i++ )
		{
			float dropChance;
			if ( i > 0 )
				dropChance = CumulativeWeaponDropPercentages[i] - CumulativeWeaponDropPercentages[i - 1];
			else
				dropChance = CumulativeWeaponDropPercentages[i];

			Log.Info( $"{WeaponDropMap[i]}: {dropChance * 100f}%" );
		}

		//Print tool drop table.
		Log.Info( "=== TOOLS DROP TABLE ===" );
		for ( var i = 0; i < CumulativeToolDropPercentages.Count; i++ )
		{
			float dropChance;
			if ( i > 0 )
				dropChance = CumulativeToolDropPercentages[i] - CumulativeToolDropPercentages[i - 1];
			else
				dropChance = CumulativeToolDropPercentages[i];

			Log.Info( $"{ToolDropMap[i]}: {dropChance * 100f}%" );
		}
	}
}
