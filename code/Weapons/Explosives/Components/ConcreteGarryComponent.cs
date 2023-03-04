namespace Grubs;

[Prefab]
public partial class ConcreteGarryComponent : ProjectileExplosiveComponent
{
	public override void OnFired( Weapon weapon, int charge )
	{
		var arcTrace = new ArcTrace( Grub, Grub.Player.MousePosition.WithZ( 1000 ) );
		Segments = arcTrace.RunTowardsWithBounces( Grub.EyeRotation.Forward.Normal * Grub.Facing, Explosive.ExplosionForceMultiplier * charge, 0, MaxBounces );
		Explosive.Position = Segments[0].StartPos;
	}
}
