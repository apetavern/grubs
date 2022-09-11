using Grubs.Player;
using Grubs.Terrain;

namespace Grubs.Crates;

/// <summary>
/// The terrain zone for picking up crates.
/// </summary>
public class PickupZone : TerrainZone
{
	/// <summary>
	/// The crate that owns this zone.
	/// </summary>
	protected BaseCrate Crate
	{
		get
		{
			if ( Parent is BaseCrate crate )
				return crate;

			Log.Error( $"{nameof( PickupZone )} did not get a {nameof( BaseCrate )} as its parent." );
			return null!;
		}
	}

	public override void Trigger( Entity entity )
	{
		base.Trigger( entity );
		if ( entity is not Grub grub )
			return;

		Crate.OnPickup( grub );
	}
}
