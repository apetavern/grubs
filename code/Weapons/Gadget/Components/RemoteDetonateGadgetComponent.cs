namespace Grubs;

[Prefab]
public partial class RemoteDetonateGadgetComponent : GadgetComponent
{
	private TimeSince _creation = 0;

	public override void ClientSpawn()
	{
		if ( Gadget.Owner.IsLocalPawn )
			_ = new UI.InputHintWorldPanel( Gadget, new List<string>() { InputAction.Fire }, new List<string>() { "Explode" } );
	}

	public override void Simulate( IClient client )
	{
		if ( Input.Pressed( InputAction.Fire ) && _creation > 0.1f )
			Gadget.Components.Get<ExplosiveGadgetComponent>()?.Explode();
	}
}
