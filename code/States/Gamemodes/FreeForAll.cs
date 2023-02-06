namespace Grubs.States;

public sealed class FreeForAll : BaseGamemode
{
	protected override void SetupParticipants( List<IClient> participants )
	{
		foreach ( var participant in participants )
			TeamManager.AddTeam( new List<IClient> { participant } );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		TeamManager.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		TeamManager.FrameSimulate( cl );
	}
}
