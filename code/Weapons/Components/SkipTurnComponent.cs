namespace Grubs;

[Prefab]
public partial class SkipTurnComponent : WeaponComponent
{
	[Prefab, Net]
	public ParticleSystem SmokeParticle { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireInstant()
	{
		var muzzle = Weapon.GetAttachment( "muzzle" );

		if ( Prediction.FirstTime && SmokeParticle is not null && muzzle is not null )
		{
			var muzzleFlash = Particles.Create( SmokeParticle.ResourcePath, muzzle.Value.Position );
			muzzleFlash.SetOrientation( 0, muzzle.Value.Rotation.Angles() );
		}

		FireFinished();
	}
}
