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
		public override bool HasReticle => false;

		/// <summary>
		/// What happens when you actually fire the weapon.
		/// </summary>
		protected override void Fire()
		{
			Log.Info( "Placed" );

			var trace = new ArcTrace( Parent, Parent.EyePos + Parent.EyeRot.Forward.Normal ).RunTowards( Parent.EyeRot.Forward.Normal, 3, 0 );
			ArcTrace.Draw( trace );
			new Projectile().WithModel( ModelPath ).WithCollisionExplosionDelay( 5 ).MoveAlongTrace( trace );
		}

		[ClientRpc]
		public override void OnFireEffects()
		{
			// Do something?
		}
	}
}
