using Grubs.Common;
using Grubs.Equipment;

namespace Grubs.Player;

[Title( "Grubs - Container" )]
[Category( "Grubs" )]
public sealed class Grub : Component
{
	[Property] public required HealthComponent Health { get; set; }
	[Property] public EquipmentComponent? ActiveEquipment { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";

	protected override void OnStart()
	{
		base.OnStart();

		ActiveEquipment?.Equip( Components.Get<SkinnedModelRenderer>() );
	}
}
