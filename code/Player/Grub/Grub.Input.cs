namespace Grubs;

public partial class Grub
{
	public float MoveInput { get; set; }
	public float LookInput { get; set; }

	public Angles LookAngles { get; set; }

	[HideInEditor]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted, HideInEditor]
	public Vector3 EyeLocalPosition { get; set; }

	[HideInEditor]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted, HideInEditor]
	public Rotation EyeLocalRotation { get; set; }

	public override Ray AimRay => new( EyePosition, EyeRotation.Forward );

	public void UpdateInputFromOwner( float moveInput, float lookInput )
	{
		MoveInput = moveInput;
		LookInput = lookInput;

		var look = (LookAngles + LookInput).Normal;
		LookAngles = look
			.WithPitch( look.pitch.Clamp( -80f, 75f ) )
			.WithRoll( 0f )
			.WithYaw( 0f );

		if ( Debug && IsTurn )
		{
			DebugOverlay.ScreenText( $"MoveInput: {MoveInput}", 13 );
			DebugOverlay.ScreenText( $"LookInput: {LookInput}", 14 );
			DebugOverlay.ScreenText( $"LookAngles: {LookAngles}", 15 );
		}
	}

	[ConVar.Replicated( "gr_debug_input" )]
	public static bool Debug { get; set; } = false;
}
