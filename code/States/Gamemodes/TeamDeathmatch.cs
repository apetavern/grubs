namespace Grubs.States;

public sealed class TeamDeathmatch : BaseGamemode
{
	protected override void SetupParticipants( List<Client> participants )
	{
		// TODO: Hard-coded to two players per team. Change this
		for ( var i = 0; i < participants.Count; i += 2 )
		{
			TeamManager.AddTeam( i + 1 < participants.Count
				? new List<Client> { participants[i], participants[i + 1] }
				: new List<Client> { participants[i] } );
		}
	}
}
