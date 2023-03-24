namespace Grubs;

[Prefab]
public partial class GadgetWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public Prefab GadgetPrefab { get; set; }

	[Prefab, Net, ResourceType( "vpcf" )]
	public string MuzzleParticle { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string UseSound { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireCursor()
	{
		FireCharged();
	}

	public override void FireInstant()
	{
		FireCharged();
	}

	public override void FireCharged()
	{
		Grub.SetAnimParameter( "fire", true );

		if ( !string.IsNullOrEmpty( MuzzleParticle ) )
		{
			var muzzle = Weapon.GetAttachment( "muzzle" );
			if ( muzzle is not null )
			{
				var muzzleFlash = Particles.Create( MuzzleParticle, muzzle.Value.Position );
				muzzleFlash.SetOrientation( 0, muzzle.Value.Rotation.Angles() );
			}
		}

		if ( !Game.IsServer )
			return;

		var gadget = PrefabLibrary.Spawn<Gadget>( GadgetPrefab );
		gadget.OnUse( Grub, Weapon, Charge );
		gadget.PlayScreenSound( UseSound );

		Charge = MinCharge;

		FireFinished();
	}
}
