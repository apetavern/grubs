namespace Grubs.Common;

/// <summary>
/// A wrapper for components to store a local reference to that component.
/// Set the static Local property to this instance in OnStart. Do not forget an IsProxy check!
/// </summary>
/// <typeparam name="T">The component type to keep a local instance of.</typeparam>
public abstract class LocalComponent<T> : Component where T : LocalComponent<T>
{
	private static T _local;
	
	public static T Local
	{
		get
		{
			if ( Game.IsEditor && _local is null )
			{
				return Game.ActiveScene.GetAllComponents<T>().FirstOrDefault( c => c.Network.IsOwner || !c.Network.IsProxy );
			}
			return _local.IsValid() ? _local : null;
		}
		protected set
		{
			_local = value;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Local = null;
	}
}
