namespace Grubs;

[Prefab]
public partial class InputHintComponent : WeaponComponent
{
	[Prefab, Net]
	public IList<string> InputActions { get; set; }

	[Prefab, Net]
	public IList<string> InputDescriptions { get; set; }
}
