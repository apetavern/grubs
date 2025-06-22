using Grubs.Common;

namespace Grubs.Systems.Pawn;

[Title( "Client" ), Category( "Grubs/Pawn" )]
public sealed class Client : LocalComponent<Client>
{
	private static readonly Logger Log = new( "Client" );
}
