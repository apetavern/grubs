namespace Grubs.Common;

/// <summary>
/// A wrapper for components to store a local reference to that component.
/// Set the static Local property to this instance in OnStart. Do not forget an IsProxy check!
/// </summary>
/// <typeparam name="T">The component type to keep a local instance of.</typeparam>
public abstract class LocalComponent<T> : Component where T : LocalComponent<T>
{
	public static T Local { get; protected set; }
	public static T GetLocal() => Local.IsValid() ? Local : null;

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Local = null;
	}
}
