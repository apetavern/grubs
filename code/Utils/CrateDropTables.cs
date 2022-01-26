using Sandbox;
using System;

namespace Grubs.Utils
{
	public static class CrateDropTables
	{
		private static bool init = false;
		private static float[] cumulativeDropPercentages;

		public enum WeaponDropTypes
		{
			BaseballBat, Railgun, Shotgun, Dynamite, Uzi,
			LandMine, Minigun, PetrolBomb, Revolver
		}

		public enum ToolDropTypes
		{
			Jetpack
		}

		public static void Init()
		{
			if ( init ) return;

			float sumTotalOfDropRates = 0f;
			foreach ( var entry in GameConfig.WeaponCrateDropChances )
			{
				sumTotalOfDropRates += entry.Value;
			}

			// Calculate the cumulative drop percentage of each Weapon in the crate.
			cumulativeDropPercentages = new float[GameConfig.WeaponCrateDropChances.Count];
			int i = 0;
			foreach ( var entry in GameConfig.WeaponCrateDropChances )
			{
				cumulativeDropPercentages[i] = (entry.Value / sumTotalOfDropRates);
				if ( i > 0 ) cumulativeDropPercentages[i] += cumulativeDropPercentages[i - 1];

				i++;
			}

			init = true;
		}

		public static WeaponDropTypes GetRandomWeaponFromCrate()
		{
			if ( !init ) Init();

			Random random = new Random( Time.Now.CeilToInt() );
			WeaponDropTypes weapon = 0;
			var roll = random.Next( 100 ) / 100f;

			while ( cumulativeDropPercentages[(int)weapon] <= roll ) weapon++;
			return weapon;
		}

		public static ToolDropTypes GetRandomToolFromCrate()
		{
			// TODO: Implement Tools
			return ToolDropTypes.Jetpack;
		}
	}
}
