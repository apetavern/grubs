using Grubs.Player;

namespace Grubs.Weapons;

[Category( "Weapons" )]
public abstract partial class GrubsWeapon : BaseCarriable
{
	public virtual string WeaponName => "";
	public virtual string ModelPath => "";
	public virtual int MaxFireCount => 1;
	public virtual HoldPose HoldPose => HoldPose.None;

	[Net, Local] public int Ammo { get; set; }
	[Net] public bool WeaponHasHat { get; set; }

	protected WormAnimator Animator;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
		WeaponHasHat = CheckWeaponForHat();
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
		base.ActiveEnd( ent, dropped );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Log.Info( WeaponHasHat );
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
