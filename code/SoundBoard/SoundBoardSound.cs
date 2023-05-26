namespace Grubs;

[GameResource( "Sound Board Sound", "sb", "A sound to play on the in-game sound board.", Icon = "grid_view" )]
public class SoundBoardSound : GameResource
{
	/// <summary>
	/// The name shown to players in the UI.
	/// </summary>
	/// <value></value>
	public string Title { get; set; }

	/// <summary>
	/// The Emoji to serve as an icon in the UI.
	/// </summary>
	/// <value></value>
	public string Icon { get; set; }
	public SoundEvent Sound { get; set; }
}
