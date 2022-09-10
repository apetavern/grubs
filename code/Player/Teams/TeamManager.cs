using Grubs.Utils;

namespace Grubs.Player;

/// <summary>
/// Manages a list of teams of grubs.
/// </summary>
[Category( "Setup" )]
public partial class TeamManager : Entity
{
	/// <summary>
	/// The single instance of this manager.
	/// <remarks>All <see cref="Grubs.States.BaseGamemode"/>s should be including a <see cref="TeamManager"/>.</remarks>
	/// </summary>
	[Net]
	public static TeamManager Instance { get; private set; } = null!;

	/// <summary>
	/// The list of all teams in this manager.
	/// </summary>
	[Net]
	public IList<Team> Teams { get; private set; }

	/// <summary>
	/// The index to the current team who is doing their turn.
	/// </summary>
	[Net]
	private int CurrentTeamNumber { get; set; }

	/// <summary>
	/// The team who is doing their turn.
	/// </summary>
	public Team CurrentTeam => Teams[CurrentTeamNumber];

	public TeamManager()
	{
		Transmit = TransmitType.Always;
		Instance = this;
	}

	/// <summary>
	/// Adds a new team
	/// </summary>
	/// <param name="clients">The clients that are a part of this team.</param>
	public void AddTeam( List<Client> clients )
	{
		Host.AssertServer();

		var team = new Team( clients, GameConfig.TeamNames[Teams.Count].ToString(), Teams.Count );
		clients.First().Pawn = team;
		Teams.Add( team );
	}

	/// <summary>
	/// Sets the team who is currently playing.
	/// </summary>
	/// <param name="teamIndex">The index of the team that is now playing.</param>
	public void SetActiveTeam( int teamIndex )
	{
		Host.AssertServer();

		CurrentTeamNumber = teamIndex;
		CurrentTeam.PickNextClient();
		CurrentTeam.PickNextGrub();
	}

	/// <summary>
	/// Cycles the team list.
	/// </summary>
	public void Cycle()
	{
		Host.AssertServer();

		if ( CurrentTeamNumber == Teams.Count - 1 )
			SetActiveTeam( 0 );
		else
			SetActiveTeam( CurrentTeamNumber + 1 );
	}
}
