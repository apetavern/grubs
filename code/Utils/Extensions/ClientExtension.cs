using Grubs.Player;
using Grubs.States;

namespace Grubs.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="Client"/>.
/// </summary>
public static class ClientExtension
{
	/// <summary>
	/// Gets the <see cref="Team"/> that the <see cref="Client"/> is a part of.
	/// </summary>
	/// <param name="client">The <see cref="Client"/> to get the team of.</param>
	/// <returns>The team the <see cref="Client"/> is a part of. Null if not in a <see cref="Team"/> or not in a <see cref="BaseGamemode"/>.</returns>
	public static Team? GetTeam( this Client client )
	{
		return TeamManager.Instance is null
			? null
			: TeamManager.Instance.Teams.FirstOrDefault( team => team.Clients.Any( participant => participant == client ) );
	}
}
