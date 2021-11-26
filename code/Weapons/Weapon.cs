using Sandbox;
using TerryForm.Pawn;
using TerryForm.States.SubStates;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
	public abstract partial class Weapon : BaseWeapon
	{
		public virtual string WeaponName => "";
		public virtual string ModelPath => "";
		public virtual int Ammo => GameConfig.AmmoDefaults[ClassInfo.Name];
		public override float PrimaryRate => 2f;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;
		public virtual bool IsFiredTurnEnding => false;
		[Net] public bool WeaponEnabled { get; set; }
		private WormAnimator Animator { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );
		}

		public void SetWeaponEnabled( bool shouldEnable )
		{
			WeaponEnabled = shouldEnable;

			Animator?.SetParam( "holdpose", shouldEnable ? (int)HoldPose : (int)HoldPose.None );
			SetVisible( shouldEnable );
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			OnActiveEffects();
		}

		public override bool CanPrimaryAttack()
		{
			if ( !WeaponEnabled )
				return false;

			return base.CanPrimaryAttack();
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
			if ( !IsFiredTurnEnding )
				return;

			/* 
			 * TODO: Let physics resolve and weapon to finish firing before ending the players turn.
			 * Temporary delay to simulate this.
			 */
			await GameTask.DelaySeconds( 1 );

			Turn.Instance?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			Animator = anim as WormAnimator;
			anim.SetParam( "holdpose", (int)HoldPose );
		}

		public void SetVisible( bool visible )
		{
			EnableDrawing = visible;
		}

		public override bool CanSecondaryAttack() => false;

		public override void AttackSecondary() { }

		public override bool CanReload() => false;

		public override void Reload() { }

		public virtual void OnOwnerKilled() { }

		[ClientRpc]
		public virtual void OnActiveEffects() { }

		public virtual void OnFireEffects() { }
	}
}
