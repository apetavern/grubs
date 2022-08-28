namespace Grubs.States;

public class FreeForAll : BaseGamemode
{
	protected override void SetupParticipants( List<Client> participants )
	{
		foreach ( var participant in participants )
			TeamManager.AddTeam( new List<Client> { participant } );
	}
}
