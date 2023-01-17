namespace Grubs.States;

public sealed class TeamDeathmatch : BaseGamemode
{
	protected override void SetupParticipants( List<IClient> participants )
	{
		// TODO: Hard-coded to two players per team. Change this
		for ( var i = 0; i < participants.Count; i += 2 )
		{
			TeamManager.AddTeam( i + 1 < participants.Count
				? new List<IClient> { participants[i], participants[i + 1] }
				: new List<IClient> { participants[i] } );
		}
	}
}
