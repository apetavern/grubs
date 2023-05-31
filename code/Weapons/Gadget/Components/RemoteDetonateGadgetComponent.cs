namespace Grubs;

[Prefab]
public partial class RemoteDetonateGadgetComponent : GadgetComponent
{
	public override void ClientSpawn()
	{
		_ = new UI.InputGlyphWorldPanel( Gadget, InputAction.Fire );
	}

	public override void Simulate( IClient client )
	{
		if ( Input.Down( InputAction.Fire ) )
			Gadget.Components.Get<ExplosiveGadgetComponent>()?.Explode();
	}
}
