namespace Grubs.Systems.Pawn.Grubs;

[Title("Grub"), Category("Grubs/Pawn")]
public sealed class Grub : Component
{
	[Sync]
	public Player Owner { get; private set; }
	
	[Rpc.Owner( NetFlags.HostOnly )]
	public void SetOwner( Player player )
	{
		Owner = player;
		Log.Info( $"Set owner of {this} to {player}." );
	}

	public override string ToString()
	{
		return $"Grub (Owner: {Owner})";
	}
}
