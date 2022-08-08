namespace Grubs.Weapons.Projectiles;

public partial class Projectile : ModelEntity
{
	public Projectile WithModel( string modelPath )
	{
		SetModel( modelPath );
		return this;
	}

	public Projectile SetPosition( Vector3 position )
	{
		Position = position;
		return this;
	}
}
