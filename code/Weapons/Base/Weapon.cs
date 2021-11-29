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
		public override float PrimaryRate => 2f;
		[Net] public int Ammo { get; set; } = 0;
		public virtual int WeaponReach { get; set; } = 100;
		public virtual bool IsFiredTurnEnding => false;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;
		private WormAnimator Animator { get; set; }
		[Net] public bool WeaponEnabled { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );
			Ammo = GameConfig.LoadoutDefaults[ClassInfo.Name];
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

			if ( Ammo == 0 )
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
			Ammo--;

			var tempTrace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward.Normal * WeaponReach ).Ignore( this ).Run();
			DebugOverlay.Line( tempTrace.StartPos, tempTrace.EndPos );

			if ( IsFiredTurnEnding )
				Turn.Instance?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );

			/* 
			 * TODO: Let physics resolve and weapon to finish firing before ending the players turn.
			 * Temporary delay to simulate this.
			 */
			await GameTask.DelaySeconds( 1 );

			Log.Info( "End turn after fired" );
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

		public void OnCarryStop()
		{
			if ( IsClient ) return;

			Owner = Owner.Owner;
			Log.Info( Owner );
		}
	}
}
