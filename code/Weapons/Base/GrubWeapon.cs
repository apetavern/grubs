using Grubs.Player;
using Grubs.States;
using Grubs.Weapons.Projectiles;

namespace Grubs.Weapons;

[Category( "Weapons" )]
public abstract partial class GrubWeapon : BaseCarriable
{
	public virtual string WeaponName => "";
	public virtual string ModelPath => "";
	public virtual string ProjectileModelPath => "";
	public virtual FiringType FiringType => FiringType.Instant;
	public virtual int MaxFireCount => 1;
	public virtual HoldPose HoldPose => HoldPose.None;
	public virtual bool HasReticle => false;
	[Net, Local] public int Ammo { get; set; }
	[Net] public bool WeaponHasHat { get; set; }
	[Net] public int Charge { get; set; }
	[Net] public bool IsFiring { get; set; } = false;

	protected GrubAnimator Animator;

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
		(GrubsGame.Current.CurrentState as BaseGamemode).UseTurn();
	}

	/// <summary>
	/// Method which acts as the entry point for firing a weapon.
	/// </summary>
	public void Fire()
	{
		(Parent as Grub).SetAnimParameter( "fire", true );

		if ( !IsServer )
			return;

		OnFire();
	}

	public override void ActiveStart( Entity ent )
	{
		if ( ent is not Grub grub )
			return;

		Animator = grub.Animator;

		EnableDrawing = true;
		Animator?.SetAnimParameter( "holdpose", (int)HoldPose );
		SetParent( grub, true );

		base.OnActive();
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		if ( ent is not Grub grub )
			return;

		EnableDrawing = false;
		ShowWeapon( grub, false );
		SetParent( Owner );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		CheckFireInput();
	}

	protected void CheckFireInput()
	{
		// Only fire if our grub is grounded and we haven't used our turn.
		var controller = (Owner as Team).ActiveGrub.Controller;
		if ( !controller.IsGrounded || (GrubsGame.Current.CurrentState as BaseGamemode).UsedTurn )
			return;

		switch ( FiringType )
		{
			case FiringType.Charged:
				if ( Input.Down( InputButton.PrimaryAttack ) )
				{
					IsFiring = true;
					Charge++;
					Charge = Charge.Clamp( 0, maxCharge );
				}

				if ( Input.Released( InputButton.PrimaryAttack ) )
				{
					IsFiring = false;
					Fire();
					Charge = 0;
				}

				break;
			case FiringType.Instant:
				if ( Input.Pressed( InputButton.PrimaryAttack ) )
					Fire();

				break;
			default:
				Log.Error( $"Got invalid firing type: {FiringType}" );
				break;
		}
	}

	/// <summary>
	/// Method to set whether the weapon should currently be visible.
	/// Used to hide the weapon while jumping, moving, etc.
	/// </summary>
	/// <param name="grub">The grub to update weapon visiblity on.</param>
	/// <param name="show">Whether the weapon should be shown.</param>
	public void ShowWeapon( Grub grub, bool show )
	{
		EnableDrawing = show;
		ShowHoldPose( show );

		if ( WeaponHasHat )
			grub.SetHatVisible( !show );
	}

	protected void ShowHoldPose( bool show )
	{
		if ( Parent is not Grub grub )
			return;

		if ( !grub.IsTurn )
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
}
