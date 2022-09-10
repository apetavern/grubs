using Grubs.Player;
using Grubs.States;

namespace Grubs.Weapons.Base;

/// <summary>
/// A weapon the grubs can use.
/// </summary>
[Category( "Weapons" )]
public abstract partial class GrubWeapon : BaseCarriable, IResolvable
{
	public bool Resolved => !IsFiring && !IsCharging;

	/// <summary>
	/// The name of the weapon.
	/// </summary>
	protected virtual string WeaponName => AssetDefinition.WeaponName;

	/// <summary>
	/// The path to the weapon model.
	/// </summary>
	protected virtual string ModelPath => AssetDefinition.Model;

	/// <summary>
	/// The way that this weapon fires.
	/// </summary>
	protected virtual FiringType FiringType => AssetDefinition.FiringType;

	/// <summary>
	/// The way that this weapon is held by the grub.
	/// </summary>
	protected virtual HoldPose HoldPose => AssetDefinition.HoldPose;

	/// <summary>
	/// The time in seconds to delay before un-equipping the weapon after use.
	/// </summary>
	protected virtual float UnequipAfter => AssetDefinition.UnequipAfter;

	/// <summary>
	/// Whether or not this weapon should have an aim reticle.
	/// </summary>
	public virtual bool HasReticle => AssetDefinition.HasReticle;

	/// <summary>
	/// The amount of times this gun can be used before being removed.
	/// </summary>
	[Net, Predicted, Local]
	public int Ammo { get; set; }

	/// <summary>
	/// The current charge the weapon has.
	/// </summary>
	[Net, Predicted]
	protected int Charge { get; private set; }

	/// <summary>
	/// Whether or not the weapon is currently being charged.
	/// </summary>
	[Net, Predicted]
	public bool IsCharging { get; private set; }

	/// <summary>
	/// Whether or not the weapon is currently being fired.
	/// </summary>
	[Net, Predicted]
	public bool IsFiring { get; protected set; }

	/// <summary>
	/// The time since the last attack started.
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceFire { get; private set; }

	/// <summary>
	/// Whether or not this weapon has a special hat associated with it.
	/// </summary>
	[Net]
	private bool WeaponHasHat { get; set; }

	/// <summary>
	/// The asset definition this weapon is implementing.
	/// </summary>
	// TODO: This is cancer https://github.com/Facepunch/sbox-issues/issues/2282
	protected WeaponAsset AssetDefinition
	{
		get => _assetDefinition;
		init
		{
			_assetDefinition = value;

			Name = value.WeaponName;
			SetModel( value.Model );
			WeaponHasHat = CheckWeaponForHat();
		}
	}
	[Net]
	private WeaponAsset _assetDefinition { get; set; }

	/// <summary>
	/// The animator of the grub that is holding the weapon.
	/// </summary>
	protected GrubAnimator? Animator;

	private const int MaxCharge = 100;

	public GrubWeapon()
	{
	}

	public GrubWeapon( WeaponAsset assetDefinition )
	{
		AssetDefinition = assetDefinition;
	}

	private bool CheckWeaponForHat()
	{
		for ( var i = 0; i < BoneCount; i++ )
		{
			if ( GetBoneName( i ) == "head" )
				return true;
		}

		return false;
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

		Animator = null;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( !IsFiring )
			CheckFireInput();
	}

	private void CheckFireInput()
	{
		// Only fire if our grub is grounded and we haven't used our turn.
		var controller = (Owner as Team)!.ActiveGrub.Controller;
		if ( !controller.IsGrounded || GrubsGame.Current.CurrentGamemode.UsedTurn )
			return;

		switch ( FiringType )
		{
			case FiringType.Charged:
				if ( Input.Down( InputButton.PrimaryAttack ) )
				{
					IsCharging = true;
					Charge++;
					Charge = Charge.Clamp( 0, MaxCharge );
					if ( Charge != MaxCharge )
						break;
					
					IsCharging = false;
					Fire();
					Charge = 0;
				}

				if ( Input.Released( InputButton.PrimaryAttack ) )
				{
					IsCharging = false;
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
	/// Called when the weapon has been fired.
	/// </summary>
	protected virtual void Fire()
	{
		IsFiring = true;
		TimeSinceFire = 0;

		var continueFiring = OnFire();

		if ( !continueFiring )
			IsFiring = false;
	}

	/// <summary>
	/// Called to do your main firing logic.
	/// </summary>
	/// <returns>Whether or not the weapon is going to continue firing.</returns>
	protected virtual bool OnFire()
	{
		(Parent as Grub)!.SetAnimParameter( "fire", true );
		GrubsGame.Current.CurrentGamemode.UseTurn();

		PlaySound( AssetDefinition.FireSound );
		return false;
	}

	/// <summary>
	/// Sets whether the weapon should be visible.
	/// </summary>
	/// <param name="grub">The grub to update weapon visibility on.</param>
	/// <param name="show">Whether or not the weapon should be shown.</param>
	public void ShowWeapon( Grub grub, bool show )
	{
		if ( show )
		{
			PlaySound( AssetDefinition.DeploySound );
		}
		EnableDrawing = show;
		Animator?.SetAnimParameter( "holdpose", show ? (int)HoldPose : (int)HoldPose.None );

		if ( WeaponHasHat )
			grub.SetHatVisible( !show );
	}
}
