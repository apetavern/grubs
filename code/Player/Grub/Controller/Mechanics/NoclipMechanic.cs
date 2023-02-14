#if DEBUG
namespace Grubs;

public class NoclipMechanic : ControllerMechanic
{
	private bool _isEnabled = false;

	[ConCmd.Admin( "noclip" )]
	public static void Noclip()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( !player.IsTurn )
			return;

		var noclip = player.ActiveGrub.Components.GetOrCreate<NoclipMechanic>();
		noclip._isEnabled = !noclip._isEnabled;
	}

	protected override bool ShouldStart()
	{
		return _isEnabled;
	}

	protected override void OnStart()
	{
		Grub.Components.RemoveAny<AirMoveMechanic>();
	}

	protected override void OnStop()
	{
		Grub.Components.GetOrCreate<AirMoveMechanic>();
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