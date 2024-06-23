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

	[Sync]
	public Guid SourceId
	{
		get => _sourceId;
		set
		{
			_sourceId = value;
			Grub = Source?.Equipment?.Grub;
			GrubGuid = Grub?.Id ?? Guid.Empty;
			GrubName = Grub?.Name ?? string.Empty;
		}
	}
	public Weapon Source => Scene.Directory.FindComponentByGuid( SourceId ) as Weapon;
	public Grub Grub { get; private set; }
	public Guid GrubGuid { get; private set; }
	public string GrubName { get; private set; }
	public GrubPlayerController PlayerController => Grub?.PlayerController;
	public virtual bool Resolved => false;

	public int Charge { get; set; }

	private Guid _sourceId;

	public void ViewReady()
	{
		Model.Enabled = true;
	}
}
