using Grubs.Common;
using Grubs.Terrain;

namespace Grubs.Systems.LevelEditing;

[Title( "Grubs - Editor Player" ), Category( "Grubs/LevelEditing" ), Icon( "edit" )]
public class EditorPlayer : LocalComponent<EditorPlayer>
{
	public EditMode EditMode { get; private set; } = EditMode.None;
	public EditorSdfShape SdfShape { get; set; } = EditorSdfShape.Circle;
	public float BrushSize { get; set; } = 0.5f;

	protected override void OnStart()
	{
		base.OnStart();

		EditMode = EditMode.Addition;
	}

	public Vector3 MousePosition { get; private set; }
	private static readonly Plane Plane = 
		new( new Vector3( 0f, 512f, 0f ), Vector3.Left );

	private const float MaxBrushSize = 256f;
	
	protected override void OnUpdate()
	{
		var cursorRay = Scene.Camera.ScreenPixelToRay( Input.UsingController
			? new Vector2( Screen.Width / 2, Screen.Height / 2 )
			: Mouse.Position );
		var endPos = Plane.Trace( cursorRay, twosided: true );
		MousePosition = endPos ?? new Vector3( 0f, 512f, 0f );
		
		var brushSize = MaxBrushSize * BrushSize;

		var t = new Transform( MousePosition, Rotation.FromYaw( 90f ), 1f );
		Gizmo.Transform = t;
		Gizmo.Draw.LineCircle( Vector3.Zero + Vector3.Backward * 64f, Vector3.Zero + Vector3.Backward * 48f, brushSize, sections: 32 );
		
		if ( Input.Down( "fire" ) )
		{
			var center = new Vector2( MousePosition.x, MousePosition.z );

			if ( Input.Down( "backflip" ) )
			{
				switch (SdfShape)
				{
					case EditorSdfShape.Circle:
						GameTerrain.Local.SubtractCircle( center, brushSize );
						break;
					case EditorSdfShape.Box:
						GameTerrain.Local.SubtractBox( center, brushSize );
						break;
				}
			}
			else
			{
				switch (SdfShape)
				{
					case EditorSdfShape.Circle:
						GameTerrain.Local.AddCircle( center, brushSize );
						break;
					case EditorSdfShape.Box:
						GameTerrain.Local.AddBox( center, brushSize );
						break;
				}
			}
		}
	}
}

public enum EditMode
{
	None,
	Addition,
	Subtraction,
}

public enum EditorSdfShape
{
	Circle,
	Box,
}
