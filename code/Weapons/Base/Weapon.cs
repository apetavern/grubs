using Sandbox;
using Grubs.Pawn;
using Grubs.States.SubStates;
using Grubs.Utils;
using Grubs.Terrain;

namespace Grubs.Weapons
{
	public abstract partial class Weapon : BaseCarriable
	{
		// Weapon settings
		public virtual string WeaponName => "";
		public virtual string ModelPath => "";
		public virtual int WeaponReach { get; set; } = 300;
		public virtual bool IsFiredTurnEnding => true;
		public virtual HoldPose HoldPose => HoldPose.Bazooka;
		public virtual int MaxQuantityFired { get; set; } = 1;
		public virtual float SecondsBetweenFired => 2.0f;
		public virtual float DamagePerShot => 25f;
		public virtual bool HasReticle { get; set; }

		// Weapon properties
		[Net] public int Ammo { get; set; }
		[Net] public bool WeaponEnabled { get; set; }
		[Net] public int QuantityFired { get; set; }
		[Net, Predicted] public TimeSince TimeSinceFired { get; set; }
		[Net] public bool WeaponHasHat { get; set; }
		protected PawnAnimator Animator { get; set; }
		public static AimReticle AimReticle { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( ModelPath );
			WeaponHasHat = CheckWeaponForHat();
			Ammo = GameConfig.LoadoutDefaults[ClassInfo.Name];
		}

		private bool CheckWeaponForHat()
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
			{
				QuantityFired++;
				OnFire();
			}

			if ( IsClient && HasReticle )
				AdjustReticle();
		}

		protected void AdjustReticle()
		{
			if ( !AimReticle.IsValid() )
				AimReticle = new();

			AimReticle.Position = Position + Parent.EyeRot.Forward.Normal * 80;
			AimReticle.Direction = Parent.EyeRot.Forward.Normal;
		}

		/// <summary>
		/// The surrounding processes of Fire();
		/// </summary>
		protected virtual void OnFire()
		{
			// Don't allow the worm to shoot this weapon if they've exceeded this turns MaxQuantityFired
			if ( QuantityFired > MaxQuantityFired )
				return;

			TimeSinceFired = 0;

			// Trigger the fire animation.
			(Parent as Worm).SetAnimBool( "fire", true );

			if ( !IsServer )
				return;

			// Create particles / screen effects.
			OnFireEffects();

			Fire();

			if ( QuantityFired >= MaxQuantityFired )
			{
				// End the turn if this weapon is turn ending.
				if ( IsFiredTurnEnding )
					Turn.Instance?.SetTimeRemaining( GameConfig.TurnTimeRemainingAfterFired );

				// Disable the weapon.
				WeaponEnabled = false;

				// Reduce weapon ammo count.
				Ammo--;
			}
		}

		/// <summary>
		/// What happens when you actually fire the weapon.
		/// </summary>
		protected virtual void Fire()
		{
			var firedTrace = Trace.Ray( Parent.EyePos, Parent.EyePos + Parent.EyeRot.Forward.Normal * WeaponReach )
				.Ignore( this )
				.Ignore( Parent )
				.Run();

			DebugOverlay.Line( firedTrace.StartPos, firedTrace.EndPos, Color.Yellow );

			switch ( firedTrace.Entity )
			{
				case Worm:
					var damage = new DamageInfo() { Damage = DamagePerShot, Position = firedTrace.StartPos, Flags = DamageFlags.Bullet };
					firedTrace.Entity.TakeDamage( damage );
					break;
			}

			if ( firedTrace.Hit )
				Terrain.Terrain.Update( new Circle( firedTrace.EndPos, 32f, SDF.MergeType.Subtract ) );

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
			ShowWeapon( worm, true );
			SetParent( worm, true );

			base.OnActive();
		}

		public override void ActiveEnd( Entity ent, bool dropped )
		{
			if ( ent is not Worm worm )
				return;

			WeaponEnabled = false;
			ShowWeapon( worm, false );
			SetParent( Owner );

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
			ShowHoldPose( show );

			if ( WeaponHasHat )
				worm.SetHatVisible( !show );
		}

		protected void ShowHoldPose( bool show )
		{
			if ( Parent is not Worm worm )
				return;

			if ( !worm.IsCurrentTurn )
				return;

			Animator?.SetParam( "holdpose", show ? (int)HoldPose : (int)HoldPose.None );
		}

		[ClientRpc]
		public virtual void OnFireEffects()
		{
			Particles.Create( "particles/muzzleflash/grubs_muzzleflash.vpcf", this, "muzzle" );
		}
	}
}
