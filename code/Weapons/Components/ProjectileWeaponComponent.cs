namespace Grubs;

// TODO: Maybe we can have something generic here instead? Not sure yet.
// Maybe a different name for this class, it is kinda similar to the other class.
[Prefab]
public partial class ProjectileWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public string ProjectilePrefabPath { get; set; }

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
		Log.Info( "Fire Cursor" );
	}

	public override void FireInstant()
	{
		Log.Info( "Fire Instant" );
	}

	public override void FireCharged()
	{
		Log.Info( "Fire Charged: " + Charge );

		if ( !Game.IsServer )
			return;

		if ( PrefabLibrary.TrySpawn<Explosive>( ProjectilePrefabPath, out var explosive ) )
		{
			explosive.OnFired( Grub, Weapon, Charge );
		}

		Grub.SetAnimParameter( "fire", true );

		IsFiring = false;
		Charge = 0;

		FireFinished();
	}
}
