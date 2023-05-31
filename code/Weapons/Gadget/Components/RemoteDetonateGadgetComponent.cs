namespace Grubs;

[Prefab]
public partial class RemoteDetonateGadgetComponent : GadgetComponent
{
	public override void Simulate( IClient client )
	{
		if ( Input.Down( InputAction.Fire ) )
			Gadget.Components.Get<ExplosiveGadgetComponent>()?.Explode();
	}
}
