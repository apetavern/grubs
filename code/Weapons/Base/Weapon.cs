using Sandbox;
using TerryForm.Pawn;
using TerryForm.States.SubStates;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
	public abstract partial class Weapon : BaseCarriable
	{
		public virtual string WeaponName => "";
		public virtual string ModelPath => "";
		[Net] public int Ammo { get; set; } = 0;
		public virtual int WeaponReach { get; set; } = 100;
		public virtual bool IsFiredTurnEnding => true;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;
		[Net] public bool WeaponEnabled { get; set; }
		protected PawnAnimator Animator { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );
			Ammo = GameConfig.LoadoutDefaults[ClassInfo.Name];
		}

		public override void Simulate( Client player )
		{
			base.Simulate( player );

			if ( Input.Down( InputButton.Attack1 ) && WeaponEnabled )
				OnFire();
		}

		/// <summary>
		/// The surrounding processes of Fire();
		/// </summary>
		protected virtual void OnFire()
		{
			// Trigger the fire animation.
			(Parent as Worm).SetAnimBool( "fire", true );

			if ( !IsServer )
				return;

			// Create particles / screen effects.
			OnFireEffects();

			Fire();

			// End the turn if this weapon is turn ending.
			if ( IsFiredTurnEnding )
				Turn.Instance?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );

			// Disable the weapon.
			WeaponEnabled = false;
		}

		/// <summary>
		/// What happens when you actually fire the weapon.
		/// </summary>
		protected virtual void Fire()
		{
			// Fire
			var firedTrace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward.Normal * WeaponReach )
				.Ignore( this )
				.Ignore( Parent )
				.Run();

			DebugOverlay.Line( firedTrace.StartPos, firedTrace.EndPos, Color.Yellow );

			if ( firedTrace.Entity is null )
				return;

			var damage = DamageInfo.FromBullet( firedTrace.EndPos, (firedTrace.StartPos - firedTrace.EndPos).Normal, 50 );
			firedTrace.Entity.TakeDamage( damage );
		}

		public override void ActiveStart( Entity ent )
		{
			if ( Ammo == 0 )
				return;

			if ( ent is not Worm worm )
				return;

			// Get the holding worm's animator & store it for later use.
			Animator = worm.GetActiveAnimator();

			WeaponEnabled = true;
			ShowHoldPose( true );
			SetParent( worm, true );
			EnableDrawing = true;

			base.OnActive();
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			if ( ent is not Worm worm )
				return;

			WeaponEnabled = false;
			ShowHoldPose( false );
			SetParent( Owner );
			EnableDrawing = false;

			base.ActiveEnd( worm, dropped );
		}

		public void HideWeapon( bool hide )
		{
			EnableDrawing = hide;
			ShowHoldPose( hide );
		}

		private void ShowHoldPose( bool show )
		{
			Animator?.SetParam( "holdpose", show ? (int)HoldPose : (int)HoldPose.None );
		}

		[ClientRpc]
		public virtual void OnFireEffects()
		{
			Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );
		}
	}
}
