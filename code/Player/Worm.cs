using Grubs.Utils;

namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Worm : AnimatedEntity
{
	[Net, Predicted]
	public WormController Controller { get; set; }

	[Net, Predicted]
	public WormAnimator Animator { get; set; }

	[Net]
	public int TeamNumber { get; set; }

	[Net]
	public bool IsTurn { get; set; } = false;

	public Worm()
	{

	}

	public Worm( int teamNumber )
	{
		TeamNumber = teamNumber;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );

		Name = Rand.FromArray( GameConfig.WormNames );
		Health = 100;

		Controller = new WormController();
		Animator = new WormAnimator();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );
	}

	public char GetTeamName()
	{
		var index = TeamNumber - 1;

		return GameConfig.TeamNames[index];
	}
}
