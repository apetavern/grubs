using Sandbox;

namespace TerryForm.Weapons
{
	public partial class Weapon : BaseWeapon
	{
		public virtual string WeaponName => "";
		public virtual string ModelPath => "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl";
		public override float PrimaryRate => 2f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );

			EnableHideInFirstPerson = false;
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			OnActiveEffects();
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			base.ActiveEnd( ent, dropped );

			OnActiveEndEffects();
		}

		public override void Simulate( Client player )
		{
			base.Simulate( player );
		}

		public override bool CanPrimaryAttack()
		{
			// TODO: Check if it's my turn, if it isn't my turn don't allow me to shoot.
			var myTurn = true;

			if ( base.CanPrimaryAttack() && myTurn )
				return true;

			return false;
		}

		public override void AttackPrimary()
		{
			OnFireEffects();

			if ( !IsServer )
				return;

			new ExplodingProjectile()
				.WithRadius( 60 )
				.SetMaxBounces( 1 )
				.WithModel( "models/rust_props/barrels/fuel_barrel.vmdl" )
				.FireFrom( Owner.EyePos + Owner.EyeRot.Forward * 70, Owner.EyeRot.Forward.Normal, 50000 );
		}

		public override bool CanSecondaryAttack() => false;

		public override void AttackSecondary() { }

		public override bool CanReload() => false;

		public override void Reload() { }

		public virtual void OnOwnerKilled() { }

		[ClientRpc]
		public virtual void OnActiveEffects() { }

		[ClientRpc]
		public virtual void OnActiveEndEffects() { }

		public virtual void OnFireEffects() { }
	}
}
