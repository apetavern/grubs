namespace Grubs;

[Category( "Grubs" )]
public partial class Gravestone : ModelEntity
{
	[Net]
	private Grub _Grub { get; set; }

	public Gravestone()
	{
		Transmit = TransmitType.Always;
	}

	public Gravestone( Grub grub ) : this()
	{
		_Grub = grub;
		SetModel( "models/gravestones/basic_gravestone/gravestone_basic.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Position = _Grub.Position;
	}
}
