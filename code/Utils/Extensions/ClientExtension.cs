using Grubs.Player;

namespace Grubs.Utils.Extensions;

public static class ClientExtension
{
	public static Team? GetTeam( this Client client )
	{
		if ( TeamManager.Instance is null )
			return null;

		return TeamManager.Instance.Teams.FirstOrDefault( team => team.Clients.Any( participant => participant == client ) );
	}
}
