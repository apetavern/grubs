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

	public override void BuildInput()
	{
		if ( Input.StopProcessing )
			return;

		MoveInput = Input.AnalogMove.y;
		LookInput = Input.AnalogMove.x;

		var cursorRay = Camera.Main.GetRay( Mouse.Position );
		var plane = new Plane( new Vector3( 0, 0, 0 ), Vector3.Left );
		var endPos = plane.Trace( cursorRay, twosided: true );
		MousePosition = endPos ?? 0f;
	}
}
