namespace Grubs.Player;

public partial class Grub
{
	public Vector2 MoveInput { get; protected set; }

	public Angles LookInput { get; protected set; }

	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted]
	public Vector3 EyeLocalPosition { get; set; }

	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted]
	public Rotation EyeLocalRotation { get; set; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	public void UpdateFromClient( Vector2 moveInput, Angles lookInput )
	{
		MoveInput = moveInput;
		LookInput = lookInput;

		if ( Debug && IsTurn )
		{
			var lineOffset = 10;

			DebugOverlay.ScreenText( $"MoveInput {MoveInput}", ++lineOffset );
			DebugOverlay.ScreenText( $"LookInput {LookInput}", ++lineOffset );
		}
	}

	[ConVar.Replicated( "gr_debug_input" )]
	public static bool Debug { get; set; } = false;
}
