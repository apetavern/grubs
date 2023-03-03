namespace Grubs;

[Prefab]
public partial class ExplosiveWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public string ExplosivePrefabPath { get; set; }

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireCursor()
	{

	}

	public override void FireInstant()
	{
		// Fire with min charge.
		FireCharged();
	}

	public override void FireCharged()
	{
		if ( !Game.IsServer )
			return;

		if ( PrefabLibrary.TrySpawn<Explosive>( ExplosivePrefabPath, out var explosive ) )
		{
			explosive.OnFired( Grub, Weapon, Charge );
		}

		Grub.SetAnimParameter( "fire", true );

		IsFiring = false;
		Charge = MinCharge;

		FireFinished();
	}
}
