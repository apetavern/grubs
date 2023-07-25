namespace Grubs;

public partial class Player
{
	/// <summary>
	/// MoveInput is fed by Input.AnalogMove in the y-axis.
	/// </summary>
	[ClientInput]
	public float MoveInput { get; set; }

	/// <summary>
	/// LookInput is fed by Input.AnalogMove in x-axis.
	/// </summary>
	[ClientInput]
	public float LookInput { get; set; }

	[ClientInput]
	public Entity ActiveWeaponInput { get; set; }

	[ClientInput]
	public Vector3 MousePosition { get; set; }

	private static readonly Plane _plane = new( new Vector3( 0f ), Vector3.Left );

	public override void BuildInput()
	{
		if ( Input.StopProcessing )
		{
			MoveInput = 0;
			LookInput = 0;
			return;
		}

		MoveInput = Input.AnalogMove.y;
		LookInput = Input.AnalogMove.x;

		var cursorRay = Camera.Main.GetRay( Mouse.Position );
		var endPos = _plane.Trace( cursorRay, twosided: true );
		MousePosition = endPos ?? 0f;
	}
}
