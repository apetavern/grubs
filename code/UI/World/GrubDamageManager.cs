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
		var grub = GetGrubByNetworkIndex( grubNetworkIdent );
		_ = new DamageNumber( grub, grub.TotalDamageTaken );
	}

	[Event( "grub.healed" )]
	public void CreateDamageNumber( int grubNetworkIdent, int healAmount )
	{
		var grub = GetGrubByNetworkIndex( grubNetworkIdent );
		_ = new DamageNumber( grub, -healAmount );
	}

	private Grub GetGrubByNetworkIndex( int netIndex )
	{
		var ent = Entity.FindByIndex( netIndex );
		return ent is not Grub grub ? null : grub;
	}
}
