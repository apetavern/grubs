using Grubs.Player;
using Grubs.Weapons.Projectiles;

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
	public virtual bool HasReticle => true;

	public AimReticle AimReticle;

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
	protected virtual void OnFire() { }

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

		if ( IsClient && HasReticle )
			AdjustReticle();
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

	protected void AdjustReticle()
	{
		if ( !AimReticle.IsValid() )
			AimReticle = new();

		var upOffset = Vector3.Up * 5f;

		var pos = Position + Parent.EyeRotation.Forward.Normal * 50;
		var dir = Parent.EyeRotation.Forward.Normal;

		var tr = Trace.Ray( Position, pos ).Size( AimReticle.RenderBounds ).WorldOnly().Run();

		if ( tr.Hit )
			pos = tr.EndPosition;

		pos += upOffset;
		AimReticle.Position = pos;
		AimReticle.Direction = dir;
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

		if ( IsClient && HasReticle && AimReticle is not null )
			AimReticle.ShowReticle = show;
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
}
