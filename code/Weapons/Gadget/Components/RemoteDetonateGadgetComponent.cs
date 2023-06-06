namespace Grubs;

[Prefab]
public partial class RemoteDetonateGadgetComponent : GadgetComponent
{
	public override void ClientSpawn()
	{
		if ( Gadget.Owner.IsLocalPawn )
			_ = new UI.InputHintWorldPanel( Gadget, new List<string>() { InputAction.Fire }, new List<string>() { "Explode" } );
	}

	public override void Simulate( IClient client )
	{
		if ( Input.Down( InputAction.Fire ) )
			Gadget.Components.Get<ExplosiveGadgetComponent>()?.Explode();
	}
}
