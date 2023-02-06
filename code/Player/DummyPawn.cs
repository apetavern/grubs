namespace Grubs.Player;

/// <summary>
/// A dummy pawn is a Client's Pawn that is required 
/// for ClientInput passthrough to a Team.
/// </summary>
public partial class DummyPawn : Entity
{
	[ClientInput]
	public Vector2 MoveInput { get; protected set; }

	[ClientInput]
	public Angles LookInput { get; protected set; }

	public override void BuildInput()
	{
		if ( Input.StopProcessing )
			return;

		MoveInput = Input.AnalogMove.y;

		var lookInput = (LookInput + Input.AnalogMove.x).Normal;
		LookInput = lookInput
			.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) )
			.WithYaw( 0f )
			.WithRoll( 0f );
	}

	public void ResetInput()
	{
		MoveInput = Vector2.Zero;
		LookInput = Angles.Zero;
	}
}
