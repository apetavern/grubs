using Grubs.Pawn;
using Sandbox;

namespace Grubs.Weapons
{
	public class LandMine : PlacedWeapon
	{
		public override string WeaponName => "Land Mine";
		public override string ModelPath => "models/weapons/landmine/landmine.vmdl";
		public override HoldPose HoldPose => HoldPose.Droppable;

		protected override void Fire()
		{
			var trace = new ArcTrace( Parent, Parent.EyePos ).RunTowardsWithBounces( Parent.EyeRot.Forward.Normal, 10, 0, 3 );

			new Projectile().MoveAlongTrace( trace ).WithModel( ModelPath );
		}

		public override void Simulate( Client player )
		{
			base.Simulate( player );

			var trace = new ArcTrace( Parent, Parent.EyePos ).RunTowardsWithBounces( Parent.EyeRot.Forward.Normal, 10, 0, 3 );
			ArcTrace.Draw( trace );
		}
	}
}
