using Grubs.Common;

namespace Grubs.Systems.Pawn;

[Title( "Player" ), Category( "Grubs/Pawn" )]
public sealed class Player : LocalComponent<Player>
{
	[Sync( SyncFlags.FromHost )]
	public Client Client { get; private set; }

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local = this;
	}

	public void SetClient( Client client )
	{
		Client = client;
	}
}
