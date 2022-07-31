namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Worm : AnimatedEntity
{
	[Net, Predicted]
	public WormController Controller { get; set; }

	[Net, Predicted]
	public WormAnimator Animator { get; set; }

	[Net]
	public bool IsTurn { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );

		Controller = new WormController();
		Animator = new WormAnimator();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );
	}
}
