using Sandbox;
using TerryForm.Pawn;
using TerryForm.States;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
	public abstract partial class Weapon : BaseWeapon
	{
		public virtual string WeaponName => "";
		public virtual string ModelPath => "";
		public override float PrimaryRate => 2f;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;

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
			var isMyTurn = (Owner as Worm)?.IsMyTurn ?? false;

			if ( base.CanPrimaryAttack() && isMyTurn )
				return true;

			return false;
		}

		public override void AttackPrimary()
		{
			OnFireEffects();

			if ( !IsServer )
				return;

			Fire();
		}

		public async virtual void Fire()
		{
			/* 
			 * TODO: Let physics resolve and weapon to finish firing before ending the players turn.
			 * Temporary delay to simulate this.
			 */
			await GameTask.DelaySeconds( 1 );

			(Game.StateHandler?.State as PlayingState)?.Turn?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );
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
