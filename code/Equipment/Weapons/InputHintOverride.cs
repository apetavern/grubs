namespace Grubs.Equipment.Weapons;

public sealed class InputHintOverride : Component
{
	[Property] public Dictionary<string, string> Inputs { get; set; } = new Dictionary<string, string>();
}
