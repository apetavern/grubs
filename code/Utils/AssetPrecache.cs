using Sandbox;

namespace Grubs.Utils
{
	public static class AssetPrecache
	{
		public static void DoPrecache()
		{
			foreach ( var resource in Resources )
			{
				Precache.Add( resource );
			}
		}

		static readonly string[] Resources =
		{
			// Models
			"models/citizenworm.vmdl",
			"models/crates/health_crate/health_crate.vmdl",
			"models/crates/tools_crate/tools_crate.vmdl",
			"models/crates/weapons_crate/weapons_crate.vmdl",
			"models/gravestones/basic_gravestone/gravestone_basic.vmdl",
			"models/tools/dynamiteplunger/dynamiteplunger.vmdl",
			"models/tools/girders/girder_long.vmdl",
			"models/tools/girders/girder_short.vmdl",
			"models/tools/jetpack/jetpack.vmdl",
			"models/tools/ninjarope/ninjarope.vmdl",
			"models/tools/ninjarope/ninjarope_hook.vmdl",
			"models/tools/teleporter/teleporter.vmdl",
			"models/weapons/airstrikes/bomb.vmdl",
			"models/weapons/airstrikes/plane.vmdl",
			"models/weapons/airstrikes/radio.vmdl",
			"models/weapons/baseballbat/baseballbat.vmdl",
			"models/weapons/bazooka/bazooka.vmdl",
			"models/weapons/dynamite/dynamite.vmdl",
			"models/weapons/goat/goat.vmdl",
			"models/weapons/grenade/grenade.vmdl",
			"models/weapons/landmine/landmine.vmdl",
			"models/weapons/minigun/minigun.vmdl",
			"models/weapons/oildrum/oildrum.vmdl",
			"models/weapons/petrolbomb/petrolbomb.vmdl",
			"models/weapons/railgun/railgun.vmdl",
			"models/weapons/shell/shell.vmdl",
			"models/weapons/shotgun/shotgun.vmdl",
			"models/weapons/revolver/revolver.vmdl",
			"models/weapons/uzi/uzi.vmdl",
			"particles/flamemodel.vmdl",
			"particles/muzzleflash/grubs_muzzleflash.vmdl",

			// Materials
			"materials/arrow.vmat",
			"materials/reticle/reticle.vmat",
			"materials/peterburroughs/dirt.vmat",
			"particles/muzzleflash/grubs_muzzleflash_gradient.vmat",
			"particles/muzzleflash/grubs_muzzleflash_flash.vmat",

			// Particles
			"particles/fire.vpcf",
			"particles/fire_loop.vpcf",
			"particles/smoke_trail.vpcf",
			"particles/muzzleflash/grubs_muzzleflash.vpcf",
			"particles/muzzleflash/grubs_muzzleflash_sparks.vpcf",
			"particles/muzzleflash/grubs_muzzleflash_sparks_impact.vpcf",
			"particles/fire/grubs_fire.vpcf",
			"particles/fire/grubs_fire_smoke.vpcf",
			"particles/fire/grubs_fire_sparks.vpcf"
		};
	}
}
