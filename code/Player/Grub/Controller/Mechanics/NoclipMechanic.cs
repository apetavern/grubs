namespace Grubs;

#if DEBUG
public class NoclipMechanic : ControllerMechanic
{
	[ConCmd.Admin( "noclip" )]
	private static void ToggleNoclip()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( !player.IsTurn )
			return;

		var noclip = player.ActiveGrub.Components.GetOrCreate<NoclipMechanic>();
		noclip._isEnabled = !noclip._isEnabled;
	}

	private bool _isEnabled = false;
	protected override bool ShouldStart() => _isEnabled;

	protected override void OnStart()
	{
		Grub.Components.RemoveAny<AirMoveMechanic>();
		Grub.Components.RemoveAny<UnstuckMechanic>();
	}

	protected override void OnStop()
	{
		Grub.Components.GetOrCreate<AirMoveMechanic>();
		Grub.Components.GetOrCreate<UnstuckMechanic>();
	}

	protected override void Simulate()
	{
		var vertical = -Grub.LookInput * Grub.Facing;
		var horizontal = -Grub.MoveInput;

		var pos = new Vector3().WithX( horizontal ) + new Vector3().WithZ( vertical );

		Controller.Position += pos * 8;
		Controller.GroundEntity = null;
	}
}
#endif
