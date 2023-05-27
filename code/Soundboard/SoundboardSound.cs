namespace Grubs;

[GameResource( "Soundboard Sound", "sb", "A sound to play on the in-game sound board.", Icon = "grid_view" )]
public class SoundboardSound : GameResource
{
	/// <summary>
	/// The name shown to players in the UI.
	/// Also used as a parameter in the ServerCmd for playing sounds. 
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// The Emoji to serve as an icon for the sound.
	/// </summary>
	public string Icon { get; set; }
	public SoundEvent Sound { get; set; }
}
