using System.Threading.Tasks;
using Grubs.Player;
using Grubs.States;

namespace Grubs.Weapons.Base;

/// <summary>
/// A weapon the grubs can use.
/// </summary>
[Category( "Weapons" )]
public abstract partial class GrubWeapon : BaseCarriable, IResolvable
{
	public bool Resolved => FireTask is null || FireTask.IsCompleted;
	
	/// <summary>
	/// The name of the weapon.
	/// </summary>
	protected virtual string WeaponName => "";
	/// <summary>
	/// The path to the weapon model.
	/// </summary>
	protected virtual string ModelPath => "";
	/// <summary>
	/// The way that this weapon fires.
	/// </summary>
	protected virtual FiringType FiringType => FiringType.Instant;
	/// <summary>
	/// The way that this weapon is held by the grub.
	/// </summary>
	protected virtual HoldPose HoldPose => HoldPose.None;
	/// <summary>
	/// Whether or not this weapon should have an aim reticle.
	/// </summary>
	public virtual bool HasReticle => false;

	/// <summary>
	/// The amount of times this gun can be used before being removed.
	/// </summary>
	[Net, Local]
	public int Ammo { get; set; }
	/// <summary>
	/// The current charge the weapon has.
	/// </summary>
	[Net]
	protected int Charge { get; private set; }
	/// <summary>
	/// Whether or not the weapon is currently being charged.
	/// </summary>
	[Net]
	public bool IsCharging { get; private set; }
	/// <summary>
	/// Whether or not this weapon has a special hat associated with it.
	/// </summary>
	[Net]
	private bool WeaponHasHat { get; set; }

	/// <summary>
	/// The animator of the grub that is holding the weapon.
	/// </summary>
	protected GrubAnimator? Animator;

	protected Task? FireTask;

	private const int MaxCharge = 100;

	public override void Spawn()
	{
		base.Spawn();

		Name = WeaponName;
		SetModel( ModelPath );
		WeaponHasHat = CheckWeaponForHat();
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
				}

				if ( Input.Released( InputButton.PrimaryAttack ) )
				{
					IsCharging = false;
					FireTask = Fire();
					Charge = 0;
				}

				break;
			case FiringType.Instant:
				if ( Input.Pressed( InputButton.PrimaryAttack ) )
					FireTask = Fire();

				break;
			default:
				Log.Error( $"Got invalid firing type: {FiringType}" );
				break;
		}
	}

	private async Task Fire()
	{
		(Parent as Grub)!.SetAnimParameter( "fire", true );

		if ( IsServer )
			await OnFire();
	}

	/// <summary>
	/// Called when the weapon has been fired.
	/// </summary>
	protected virtual async Task OnFire()
	{
		Host.AssertServer();

		GrubsGame.Current.CurrentGamemode.UseTurn();
	}

	/// <summary>
	/// Sets whether the weapon should be visible.
	/// </summary>
	/// <param name="grub">The grub to update weapon visibility on.</param>
	/// <param name="show">Whether or not the weapon should be shown.</param>
	public void ShowWeapon( Grub grub, bool show )
	{
		EnableDrawing = show;
		Animator?.SetAnimParameter( "holdpose", show ? (int)HoldPose : (int)HoldPose.None );

		if ( WeaponHasHat )
			grub.SetHatVisible( !show );
	}
}
