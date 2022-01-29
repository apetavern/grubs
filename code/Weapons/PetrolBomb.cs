using Grubs.Pawn;
using Grubs.States.SubStates;

namespace Grubs.Weapons
{
	public class PetrolBomb : ArcWeapon
	{
		public override string WeaponName => "Petrol Bomb";
		public override string ModelPath => "models/weapons/petrolbomb/petrolbomb.vmdl";
		public override string ProjectileModel => ModelPath;
		public override HoldPose HoldPose => HoldPose.Throwable;

		protected override void Fire()
		{
			var trace = new ArcTrace( Parent, Parent.EyePos ).RunTowards( Parent.EyeRot.Forward.Normal, ComputedForce, Turn.Instance?.WindForce ?? 0 );

			new Projectile().MoveAlongTrace( trace ).WithModel( ProjectileModel ).SetCollisionReaction( ProjectileCollisionReaction.Incendiary );
		}
	}
}
