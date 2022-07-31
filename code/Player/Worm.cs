namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Worm : AnimatedEntity
{
	[Net, Predicted]
	public WormController Controller { get; set; }

	[Net, Predicted]
	public WormAnimator Animator { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );

		Controller = new WormController();
		Animator = new WormAnimator();

		Position = new Vector3( 0, 0, 1024 );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );
	}
}
