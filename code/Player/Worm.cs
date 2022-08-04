using Grubs.Utils;
using Grubs.Weapons;

namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Worm : AnimatedEntity
{
	[Net, Predicted]
	public WormController Controller { get; set; }

	[Net, Predicted]
	public WormAnimator Animator { get; set; }

	[Net, Predicted]
	public GrubsWeapon ActiveChild { get; set; }

	[Net, Predicted]
	public GrubsWeapon LastActiveChild { get; set; }

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

		if ( IsTurn )
			SimulateActiveChild( cl, ActiveChild );
	}

	public virtual void SimulateActiveChild( Client client, GrubsWeapon child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
			return;

		LastActiveChild.Simulate( client );
	}

	public virtual void OnActiveChildChanged( GrubsWeapon previous, GrubsWeapon next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	public void EquipWeapon( GrubsWeapon weapon )
	{
		ActiveChild = weapon;
	}

	public char GetTeamName()
	{
		var index = TeamNumber - 1;

		return GameConfig.TeamNames[index];
	}
}
