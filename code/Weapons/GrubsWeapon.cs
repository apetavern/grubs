using Grubs.Player;
using Grubs.States;
using Grubs.Weapons.Projectiles;
using Grubs.UI.World;

namespace Grubs.Weapons;

[Category( "Weapons" )]
public abstract partial class GrubsWeapon : BaseCarriable
{
	public virtual string WeaponName => "";
	public virtual string ModelPath => "";
	public virtual string ProjectileModelPath => "";
	public virtual FiringType FiringType => FiringType.Instant;
	public virtual int MaxFireCount => 1;
	public virtual HoldPose HoldPose => HoldPose.None;
	public virtual bool HasReticle { get; set; }
	[Net, Local] public int Ammo { get; set; }
	[Net] public bool WeaponHasHat { get; set; }
	[Net] public int Charge { get; set; }
	[Net] public bool IsFiring { get; set; } = false;

	protected WormAnimator Animator;

	private readonly int maxCharge = 100;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( ModelPath );
		WeaponHasHat = CheckWeaponForHat();

	}

	/// <summary>
	/// Server-side method to implement that controls what happens 
	/// when the Fire button is pressed.
	/// </summary>
	protected virtual void OnFire()
	{
		(GrubsGame.Current.CurrentState as PlayState).UseTurn();
	}

	/// <summary>
	/// Method which acts as the entry point for firing a weapon.
	/// </summary>
	public void Fire()
	{
		(Parent as Worm).SetAnimParameter( "fire", true );

		if ( !IsServer )
			return;

		OnFire();
	}

	public override void ActiveStart( Entity ent )
	{
		if ( ent is not Worm worm )
			return;

		Animator = worm.Animator;

		EnableDrawing = true;
		Animator?.SetAnimParameter( "holdpose", (int)HoldPose );
		SetParent( worm, true );

		base.OnActive();
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		if ( ent is not Worm worm )
			return;

		EnableDrawing = false;
		ShowWeapon( worm, false );
		SetParent( Owner );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		CheckFireInput();
		CheckReticle();
	}

	protected void CheckFireInput()
	{
		// Only fire is our worm is grounded.
		var controller = (Owner as GrubsPlayer).ActiveWorm.Controller;
		if ( !controller.IsGrounded )
			return;

		if ( FiringType is FiringType.Charged )
		{
			if ( Input.Down( InputButton.PrimaryAttack ) )
			{
				IsFiring = true;
				Charge++;
				Charge = Charge.Clamp( 0, maxCharge );
			}

			if ( Input.Released( InputButton.PrimaryAttack ) )
			{
				IsFiring = false;
				Log.Info( $"Fired {this} weapon charged. Final Charge: {Charge}" );
				Charge = 0;
				Fire();
			}
		}
		else
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				Log.Info( $"Fired {this} weapon instantly." );
				Fire();
			}
		}
	}

	/// <summary>
	/// Method to set whether the weapon should currently be visible.
	/// Used to hide the weapon while jumping, moving, etc.
	/// </summary>
	/// <param name="worm">The worm to update weapon visiblity on.</param>
	/// <param name="show">Whether the weapon should be shown.</param>
	public void ShowWeapon( Worm worm, bool show )
	{
		EnableDrawing = show;
		ShowHoldPose( show );

		if ( WeaponHasHat )
			worm.SetHatVisible( !show );

		// if ( IsClient && HasReticle && AimReticle is not null )
		// AimReticle.ShowReticle = show;
		// AimReticle.ShowReticle = show;
	}

	protected void ShowHoldPose( bool show )
	{
		if ( Parent is not Worm worm )
			return;

		if ( !worm.IsTurn )
			return;

		Animator?.SetAnimParameter( "holdpose", show ? (int)HoldPose : (int)HoldPose.None );
	}

	private bool CheckWeaponForHat()
	{
		for ( int i = 0; i < BoneCount; i++ )
			if ( GetBoneName( i ) == "head" )
				return true;

		return false;
	}

	private void CheckReticle()
	{

		if ( !HasReticle && IsClient )
		{
			new AimReticle( (Local.Pawn as GrubsPlayer).ActiveWorm );
			HasReticle = true;
		}
	}
}
