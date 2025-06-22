using Grubs.Common;

namespace Grubs.Systems.LevelEditing;

[Title( "Grubs - Editor Player" ), Category( "Grubs/LevelEditing" ), Icon( "edit" )]
public class EditorPlayer : LocalComponent<EditorPlayer>
{
	public EditMode EditMode { get; private set; } = EditMode.None;
}

public enum EditMode
{
	None,
	Addition,
	Subtraction,
}
