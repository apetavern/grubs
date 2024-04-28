using Grubs.Common;
using Grubs.Pawn;
using Grubs.Pawn.Controller;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Projectile" ), Category( "Equipment" )]
public abstract class ProjectileComponent : Component, IResolvable
{
	[Property] public float ProjectileSpeed { get; set; } = 4f;
	[Property] public required SkinnedModelRenderer Model { get; set; }

	public WeaponComponent Source { get; set; }
	public Grub Grub => Source?.Equipment.Grub;
	public GrubPlayerController PlayerController => Grub?.PlayerController;
	public virtual bool Resolved => false;

	public int Charge { get; set; }

	public void ViewReady()
	{
		Model.Enabled = true;
	}
}
