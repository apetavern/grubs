namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Gravestone : ModelEntity
{
	[Net]
	private Worm Worm { get; set; }

	public Gravestone()
	{
		Transmit = TransmitType.Always;
	}

	public Gravestone( Worm worm ) : this()
	{
		Worm = worm;
		SetModel( "models/gravestones/basic_gravestone/gravestone_basic.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Position = Worm.Position;
	}
}
