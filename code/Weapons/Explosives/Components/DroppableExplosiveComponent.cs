namespace Grubs;

[Prefab]
public partial class DroppableExplosiveComponent : ExplosiveComponent
{
	public override void OnFired( Weapon weapon, int charge )
	{
		base.OnFired( weapon, charge );

		var desiredPosition = Grub.Position + (Grub.EyeRotation.Forward.Normal * Grub.Facing * 40f);
		var tr = Trace.Ray( desiredPosition, desiredPosition ).Ignore( Grub ).Run(); // This trace is incorrect, should be from position -> desired position.
		Explosive.Position = tr.EndPosition;
		Explosive.Velocity = (Grub.EyeRotation.Forward.Normal * Grub.Facing).WithY( 0f );
	}
}
