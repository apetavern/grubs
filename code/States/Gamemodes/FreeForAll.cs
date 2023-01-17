namespace Grubs.States;

public sealed class FreeForAll : BaseGamemode
{
	protected override void SetupParticipants( List<IClient> participants )
	{
		foreach ( var participant in participants )
			TeamManager.AddTeam( new List<IClient> { participant } );
	}
}
