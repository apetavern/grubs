using GridAStar;

namespace Grubs;

public partial class Terrain
{
	public static int WorldLength => GamemodeSystem.Instance.Terrain.WorldTextureLength;
	public static int WorldHeight => GamemodeSystem.Instance.Terrain.WorldTextureHeight;
	public static BBox WorldBox => new BBox( new Vector3( WorldLength / 2f, 10f, WorldHeight ), new Vector3( -WorldLength / 2f, 0, 0 ) );
	public static JumpDefinition NormalJump = new JumpDefinition( "jump", 125f, 240f, ControllerMechanic.Gravity, 2 );
	public static JumpDefinition BackFlipJump = new JumpDefinition( "backflip", 50f, 240f * 1.75f, ControllerMechanic.Gravity, 2 );

	public static async Task GenerateGrid()
	{
		var builder = new GridAStar.GridBuilder()
			.WithBounds( Vector3.Zero, WorldBox, Rotation.Identity )
			.WithHeightClearance( 24f ) // EyeHeight is set to 28 but everywhere I can find uses 24f
			.WithWidthClearance( GrubController.BodyGirth )
			.WithoutTags( "trigger" )
			.WithEdgeNeighbourCount( 2 )
			.WithStepSize( SquirmMechanic.StepSize )
			.WithStandableAngle( SquirmMechanic.GroundAngle )
			.WithMaxDropHeight( 150 )
			.AddJumpDefinition( NormalJump )
			.AddJumpDefinition( BackFlipJump );

		await builder.Create( 1, printInfo: true );
	}

	[ConCmd.Admin("grub_grid")]
	static void GrubGridDebug()
	{
		GenerateGrid().Wait();
	}
}
