namespace Grubs;

[Prefab]
public partial class InputHintComponent : WeaponComponent
{
	[Prefab, Net]
	public IList<string> InputActions { get; set; }

	[Prefab, Net]
	public IList<string> InputDescriptions { get; set; }

	protected override void OnActivate()
	{
		// We can't use a dictionary since they are not supported in the Prefab Editor.
		// Basically we need to ensure for every input action we have a description.
		Assert.True( InputActions.Count == InputDescriptions.Count );
	}
}
