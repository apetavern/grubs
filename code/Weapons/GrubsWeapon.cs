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

	protected WormAnimator Animator;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
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
	}
}
