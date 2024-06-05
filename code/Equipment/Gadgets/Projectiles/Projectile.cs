using Grubs.Common;
using Grubs.Equipment.Weapons;
using Grubs.Pawn;
using Grubs.Pawn.Controller;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Projectile" ), Category( "Equipment" )]
public abstract class Projectile : Component, IResolvable
{
	[Property] public float ProjectileSpeed { get; set; } = 4f;
	[Property] public required SkinnedModelRenderer Model { get; set; }

	public Weapon Source
	{
		get
		{
			return _source;
		}
		set
		{
			_source = value;
			Grub = Source?.Equipment?.Grub;
		}
	}
	public Grub Grub { get; private set; }
	public GrubPlayerController PlayerController => Grub?.PlayerController;
	public virtual bool Resolved => false;

	public int Charge { get; set; }

	private Weapon _source;

	public void ViewReady()
	{
		Model.Enabled = true;
	}
}
