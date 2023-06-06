namespace Grubs;

[Prefab]
public partial class InputHintOverrideComponent : EntityComponent<Weapon>
{
	[Prefab, Net]
	public IList<string> InputActions { get; set; }

	[Prefab, Net]
	public IList<string> InputDescriptions { get; set; }
}
