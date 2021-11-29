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
		public virtual bool IsFiredTurnEnding => false;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;
		[Net] public bool WeaponEnabled { get; set; }
		private PawnAnimator Animator { get; set; }

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
				Fire();
		}

		protected virtual void Fire()
		{
			(Parent as Worm).SetAnimBool( "fire", true );

			if ( !IsServer )
				return;

			OnFireEffects();

			var tempTrace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward.Normal * WeaponReach ).Ignore( this ).Run();
			DebugOverlay.Line( tempTrace.StartPos, tempTrace.EndPos );

			if ( IsFiredTurnEnding )
				Turn.Instance?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );

			WeaponEnabled = false;
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
