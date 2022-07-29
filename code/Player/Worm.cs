namespace Grubs.Player;

public partial class Worm : AnimatedEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );
	}
}
