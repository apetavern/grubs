namespace Grubs.Player;

public partial class Grub
{
	[ClientInput]
	public Vector2 MoveInput { get; protected set; }

	[ClientInput]
	public Angles LookInput { get; protected set; }

	[ClientInput]
	public Entity ActiveWeaponInput { get; set; } = null!;

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

	public override void BuildInput()
	{
		if ( Input.StopProcessing )
			return;

		if ( !IsTurn )
			return;

		MoveInput = Input.AnalogMove.y;

		var lookInput = (LookInput + Input.AnalogMove.x).Normal;
		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
	}
}
