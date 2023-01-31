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
	public float CurrentEyeHeight { get; set; } = 32f;

	public Vector3 BaseVelocity { get; set; }
	public Vector3 LastVelocity { get; set; }
	public Vector3 WishVelocity { get; set; }
	public Entity GroundEntity { get; set; } = null!;
	public Entity LastGroundEntity { get; set; } = null!;
	public Vector3 GroundNormal { get; set; }
	public float CurrentGroundAngle { get; set; }

	public static float BodyGirth => 24f;
	public static float EyeHeight => 32f;

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

	public virtual void Simulate( IClient client )
	{
		SimulateEyes();

		if ( Debug && Grub.IsTurn )
		{
			var hull = Hull;
			DebugOverlay.Box( Position, hull.Mins, hull.Maxs, Color.Red );
			DebugOverlay.Box( Position, hull.Mins, hull.Maxs, Color.Blue );

			var lineOffset = 0;

			DebugOverlay.ScreenText( $"Player Controller", ++lineOffset );
			DebugOverlay.ScreenText( $"        Position: {Position}", ++lineOffset );
			DebugOverlay.ScreenText( $"        Velocity: {Velocity}", ++lineOffset );
			DebugOverlay.ScreenText( $"    BaseVelocity: {BaseVelocity}", ++lineOffset );
			DebugOverlay.ScreenText( $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]", ++lineOffset );
			DebugOverlay.ScreenText( $"           Speed: {Velocity.Length}", ++lineOffset );

		}
	}

	[ConVar.Replicated( "gr_debug_playercontroller" )]
	public static bool Debug { get; set; } = false;
}
