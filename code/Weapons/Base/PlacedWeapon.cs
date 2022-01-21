using Sandbox;
using Grubs.Pawn;
using Grubs.States.SubStates;
using Grubs.Utils;
using Grubs.Terrain;

namespace Grubs.Weapons
{
	public abstract partial class PlacedWeapon : Weapon
	{
		// Weapon settings
		public override string WeaponName => "";
		public override string ModelPath => "";
		public override bool IsFiredTurnEnding => true;
		public override HoldPose HoldPose => HoldPose.Throwable;

		/// <summary>
		/// What happens when you actually fire the weapon.
		/// </summary>
		protected override void Fire()
		{
			Log.Info( "Placed" );
		}

		[ClientRpc]
		public virtual void OnFireEffects()
		{
			// Do something?
		}
	}
}
