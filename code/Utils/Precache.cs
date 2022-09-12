namespace Grubs.Utils;

/// <summary>
/// A utility class to precache everything that Grubs uses.
/// </summary>
public static class Precache
{
	/// <summary>
	/// Adds all items to the precache.
	/// </summary>
	public static void Run()
	{
		//
		// Models
		//
		// Grub
		Sandbox.Precache.Add( "models/citizenworm.vmdl" );

		// Weapons
		Sandbox.Precache.Add( "models/weapons/baseballbat/baseballbat.vmdl" );
		Sandbox.Precache.Add( "models/weapons/bazooka/bazooka.vmdl" );
		Sandbox.Precache.Add( "models/weapons/railgun/railgun.vmdl" );
		Sandbox.Precache.Add( "models/weapons/grenade/grenade.vmdl" );
		Sandbox.Precache.Add( "models/weapons/minigun/minigun.vmdl" );
		Sandbox.Precache.Add( "models/weapons/petrolbomb/petrolbomb.vmdl" );
		Sandbox.Precache.Add( "models/weapons/revolver/revolver.vmdl" );
		Sandbox.Precache.Add( "models/weapons/shotgun/shotgun.vmdl" );
		Sandbox.Precache.Add( "models/weapons/uzi/uzi.vmdl" );
		Sandbox.Precache.Add( "models/tools/dynamiteplunger/dynamiteplunger.vmdl" );

		// Crates
		Sandbox.Precache.Add( "models/crates/health_crate/health_crate.vmdl" );
		Sandbox.Precache.Add( "models/crates/weapons_crate/weapons_crate.vmdl" );

		//
		// Particles
		//
		// Gun/Projectile
		Sandbox.Precache.Add( "particles/guntrace/guntrace.vpcf" );
		Sandbox.Precache.Add( "particles/muzzleflash/grubs_muzzleflash.vpcf" );
		Sandbox.Precache.Add( "particles/muzzleflash/grubs_muzzleflash_sparks.vpcf" );
		Sandbox.Precache.Add( "particles/muzzleflash/grubs_muzzleflash_sparks_impact.vpcf" );
		Sandbox.Precache.Add( "particles/smoke_trail.vpcf" );

		// Explosion
		Sandbox.Precache.Add( "particles/explosion/grubs_explosion_base.vpcf" );
		Sandbox.Precache.Add( "particles/explosion/grubs_explosion_fire.vpcf" );
		Sandbox.Precache.Add( "particles/explosion/grubs_explosion_shockwave.vpcf" );
		Sandbox.Precache.Add( "particles/explosion/grubs_explosion_smoke.vpcf" );
		Sandbox.Precache.Add( "particles/explosion/grubs_explosion_sparks.vpcf" );

		//
		// Sounds
		//
		// Baseball bat
		Sandbox.Precache.Add( "weapons/rust_flashlight/sounds/rust_flashlight.attack.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Bazooka
		Sandbox.Precache.Add( "sounds/physics/breaking/break_wood_plank.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.dryfire.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Gibgun
		Sandbox.Precache.Add( "sounds/physics/breaking/break_wood_plank.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Grenade
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.dryfire.sound" );
		Sandbox.Precache.Add( "sounds/physics/breaking/break_wood_plank.sound" );
		Sandbox.Precache.Add( "weapons/rust_flashlight/sounds/rust_flashlight.attack.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Minigun
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.shoot.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Petrol bomb
		Sandbox.Precache.Add( "audio/sfx/gun/revolver_fire.sound" );
		Sandbox.Precache.Add( "sounds/physics/physics.glass.shard.impact.sound" );
		Sandbox.Precache.Add( "weapons/rust_flashlight/sounds/rust_flashlight.attack.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Revolver
		Sandbox.Precache.Add( "weapons/rust_pistol/sound/rust_pistol.shoot.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Shotgun
		Sandbox.Precache.Add( "weapons/rust_pumpshotgun/sounds/rust_pumpshotgun.shoot.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );

		// Uzi
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.shoot.sound" );
		Sandbox.Precache.Add( "weapons/rust_smg/sounds/rust_smg.deploy.sound" );
	}
}
