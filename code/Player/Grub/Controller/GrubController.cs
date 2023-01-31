namespace Grubs.Player;

public partial class GrubController : EntityComponent<Grub>
{
	public Grub Grub => Entity;

	public Vector3 Position
	{
		get => Grub.Position;
		set => Grub.Position = value;
	}

	public Vector3 Velocity
	{
		get => Grub.Velocity;
		set => Grub.Velocity = value;
	}

	[Net, Predicted]
	public float CurrentEyeHeight { get; set; } = 24f;

	public static float BodyGirth => 32f;
	public static float EyeHeight => 24f;

	public BBox Hull
	{
		get
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, CurrentEyeHeight );

			return new BBox( mins, maxs );
		}
	}

	protected void SimulateEyes()
	{
		Grub.EyeRotation = Grub.LookInput.ToRotation();
		Grub.EyeLocalPosition = Vector3.Up * CurrentEyeHeight;
	}
}
