using Grubs.Terrain;

namespace Grubs.Systems.LevelEditing;

/// <summary>
/// The definition for a Grubs custom level.
/// </summary>
public class LevelDefinition
{
	#region Meta
	
	/// <summary>
	/// Machine-readable Guid for the level.
	/// </summary>
	public Guid Id { get; set; }
	
	/// <summary>
	/// Human-readable name for the level.
	/// </summary>
	public string DisplayName { get; set; }

	/// <summary>
	/// Short level description.
	/// </summary>
	public string Description { get; set; }
	
	/// <summary>
	/// A list of tags represented the level's characteristics.
	/// </summary>
	public List<string> Tags { get; set; }

	/// <summary>
	/// The Steam user who created this level.
	/// </summary>
	public Friend CreatedBy { get; set; }
	
	/// <summary>
	/// The DateTime this level was first created.
	/// </summary>
	public DateTime CreatedOn { get; set; }
	
	/// <summary>
	/// The DateTime this level was last updated.
	/// </summary>
	public DateTime LastUpdated { get; set; }
	
	#endregion Meta
	
	#region LevelData
	
	public TerrainSize TerrainSize { get; set; }
	
	#endregion
}
