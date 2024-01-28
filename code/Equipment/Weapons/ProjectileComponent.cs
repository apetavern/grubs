using Grubs.Player;

namespace Grubs.Equipment.Weapons;

public class ProjectileComponent : Component
{
	[Property] public float ProjectileSpeed { get; set; } = 4f;

	public WeaponComponent? Source { get; set; }
	public int Charge { get; set; }

	public Vector3 Velocity { get; set; }

	protected override void OnStart()
	{
		if ( Source is null )
			return;

		var equipmentMuzzle = Source.Equipment.Model.GetAttachment( "muzzle" );
		if ( equipmentMuzzle is null )
			return;

		Transform.Position = equipmentMuzzle.Value.Position;
		Transform.Rotation = equipmentMuzzle.Value.Rotation;

		if ( Source.Equipment.Grub is not { } grub )
			return;

		var controller = grub.PlayerController;
		Velocity = controller.EyeRotation.Forward.Normal * controller.Facing * Charge * ProjectileSpeed;
		var body = Components.Create<Rigidbody>();
		body.ApplyImpulse( Velocity );
	}

	protected override void OnFixedUpdate()
	{
	}
}
