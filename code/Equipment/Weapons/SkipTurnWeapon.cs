using Grubs.Helpers;

namespace Grubs.Equipment.Weapons
{
	[Title( "Grubs - Skip Turn Weapon" ), Category( "Equipment" )]
	public class SkipTurnWeapon : Weapon
	{
		[Property] public ParticleSystem UseParticles { get; set; }

		protected override void FireImmediate()
		{
			if ( Equipment.Grub is not { } grub )
				return;

			FireEffects();
			FireFinished();
		}

		[Broadcast]
		private void FireEffects()
		{
			var muzzle = GetMuzzleTransform();

			if ( UseParticles is not null )
			{
				var smoke = ParticleHelper.Instance.PlayInstantaneous( UseParticles, muzzle );
				smoke.SetControlPoint( 1, 2f ); // Rise distance
			}
		}
	}
}
