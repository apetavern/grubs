namespace Grubs;

public class GrubDamageManager
{
	public GrubDamageManager()
	{
		Event.Register( this );
	}

	[GrubsEvent.Grub.Damaged]
	public void CreateDamageNumber( int grubNetworkIdent )
	{
		var ent = Entity.FindByIndex( grubNetworkIdent );
		if ( ent is not Grub grub )
			return;

		_ = new DamageNumber( grub, grub.TotalDamageTaken );
	}
}
