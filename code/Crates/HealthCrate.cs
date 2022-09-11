using Grubs.Player;

namespace Grubs.Crates;

/// <summary>
/// A crate that heals Grubs on touch.
/// </summary>
public class HealthCrate : BaseCrate
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/crates/health_crate/health_crate.vmdl" );
	}

	public override void OnPickup( Grub grub )
	{
		if ( grub.GiveHealth( 50 ) )
			base.OnPickup( grub );
	}
}
