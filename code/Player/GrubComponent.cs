using Grubs.Components;

namespace Grubs.Player;

[Title( "Grubs - Container" )]
[Category( "Grubs" )]
public sealed class Grub : Component
{
	[Property] public required HealthComponent Health { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";
}
