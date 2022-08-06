using Grubs.Player;

namespace Grubs.Weapons;

[Category( "Weapons" )]
public abstract partial class GrubsWeapon : BaseCarriable
{
	public virtual string WeaponName => "";
	public virtual string ModelPath => "";
	public virtual int MaxFireCount => 1;
	public virtual HoldPose HoldPose => HoldPose.None;
	public virtual bool HasReticle => true;

	public static AimReticle AimReticle;

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
		if ( ent is not Worm worm )
			return;

		EnableDrawing = false;
		ShowWeapon( worm, false );
		SetParent( Owner );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsClient && HasReticle )
			AdjustReticle();
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
