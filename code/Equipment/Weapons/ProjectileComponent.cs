using Grubs.Player;
using Grubs.Player.Controller;

namespace Grubs.Equipment.Weapons;

[Title( "Grub - Projectile" )]
[Category( "Equipment" )]
public abstract class ProjectileComponent : Component
{
	[Property] public float ProjectileSpeed { get; set; } = 4f;
	[Property] public required SkinnedModelRenderer Model { get; set; }

	public WeaponComponent? Source { get; set; }
	public Grub? Grub => Source?.Equipment.Grub;
	public GrubPlayerController? PlayerController => Grub?.PlayerController;

	public int Charge { get; set; }

	public void ViewReady()
	{
		Model.Enabled = true;
	}
}
