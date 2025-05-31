using Grubs.Helpers;
using Grubs.Systems.Particles;

namespace Grubs.Equipment.Weapons
{
	[Title( "Grubs - Skip Turn Weapon" ), Category( "Equipment" )]
	public class SkipTurnWeapon : Weapon
	{
		protected override void FireImmediate()
		{
			if ( Equipment.Grub is not { } grub )
				return;

			FireEffects();
			FireFinished();
		}

		[Rpc.Broadcast]
		private void FireEffects()
		{
			var muzzle = GetMuzzleTransform();

			SmokeStackParticles.Spawn()
				.SetWorldPosition( muzzle.Position );
		}
	}
}
