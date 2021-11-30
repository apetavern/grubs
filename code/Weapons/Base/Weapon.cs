using Sandbox;
using TerryForm.Pawn;
using TerryForm.States.SubStates;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
	public abstract partial class Weapon : BaseCarriable
	{
		// Weapon settings
		public virtual string WeaponName => "";
		public virtual string ModelPath => "";
		public virtual int WeaponReach { get; set; } = 100;
		public virtual bool IsFiredTurnEnding => true;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;
		public virtual int MaxQuantityFired { get; set; } = 1;
		public virtual float SecondsBetweenFired => 5.0f;

		// Weapon properties
		[Net, Predicted] public int Ammo { get; set; }
		[Net, Predicted] public bool WeaponEnabled { get; set; }
		[Net, Predicted] public int QuantityFired { get; set; }
		[Net, Predicted] public TimeSince TimeSinceFired { get; set; }
		private bool WeaponHasHat { get; set; }
		protected PawnAnimator Animator { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );
			WeaponHasHat = CheckWeaponForHats();
			Ammo = GameConfig.LoadoutDefaults[ClassInfo.Name];
		}

		private bool CheckWeaponForHats()
		{
			for ( int i = 0; i < BoneCount; i++ )
			{
				if ( GetBoneName( i ) == "head" )
					return true;
			}

			return false;
		}

		public override void Simulate( Client player )
		{
			base.Simulate( player );

			if ( Input.Down( InputButton.Attack1 ) && WeaponEnabled && TimeSinceFired > SecondsBetweenFired )
				OnFire();
		}

		/// <summary>
		/// The surrounding processes of Fire();
		/// </summary>
		protected virtual void OnFire()
		{
			// Don't allow the worm to shoot this weapon if they've exceeded this turns MaxQuantityFired
			if ( QuantityFired >= MaxQuantityFired )
				return;

			TimeSinceFired = 0;

			// Trigger the fire animation.
			(Parent as Worm).SetAnimBool( "fire", true );

			if ( !IsServer )
				return;

			// Create particles / screen effects.
			OnFireEffects();

			Fire();

			if ( QuantityFired < MaxQuantityFired )
			{
				// End the turn if this weapon is turn ending.
				if ( IsFiredTurnEnding )
					Turn.Instance?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );

				// Disable the weapon.
				WeaponEnabled = false;
			}

			QuantityFired++;
		}

		/// <summary>
		/// What happens when you actually fire the weapon.
		/// </summary>
		protected virtual void Fire()
		{
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

			SetParent( worm, true );
			WeaponEnabled = true;
			ShowWeapon( worm, true );

			base.OnActive();
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			if ( ent is not Worm worm )
				return;

			SetParent( Owner );
			WeaponEnabled = false;
			ShowWeapon( worm, false );

			// Weapon has been put back into the inventory, reset QuantityFired. 
			// This creates an exploit that will allow the player to switch guns to 
			// reset the quantity fired. This will be fixed when we disallow weapon selection
			// after having shot a bullet.
			QuantityFired = 0;

			base.ActiveEnd( worm, dropped );
		}

		public void ShowWeapon( Worm worm, bool show )
		{
			EnableDrawing = show;
			ShowHoldPose( worm, show );

			worm.SetHatVisible( !show );
		}

		private void ShowHoldPose( Worm worm, bool show )
		{
			if ( !worm.IsCurrentTurn )
				return;

			Animator?.SetParam( "holdpose", show ? (int)HoldPose : (int)HoldPose.None );
		}

		[ClientRpc]
		public virtual void OnFireEffects()
		{
			Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );
		}
	}
}
