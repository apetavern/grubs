namespace Grubs.Utils;

public static class SoundExtensions
{
	/// <summary>
	/// Fade a sound out over time.
	/// </summary>
	/// <param name="sound"></param>
	/// <param name="fadeRate">How quickly, in seconds, that the sound will be lowered.</param>
	/// <param name="fadeTime">The total period of time the fading occurs.</param>
	/// <param name="fadeMultiplier">The intensity of the fading.</param>
	/// <param name="startVolume">The volume to start fading from.</param>
	public static async void FadeOut( this Sound sound, float fadeRate, float fadeTime = 1, float fadeMultiplier = 1f, float startVolume = 1.0f )
	{
		var untilDone = 0f;
		var currentVolume = startVolume;

		while ( untilDone <= fadeTime )
		{
			currentVolume -= Time.Delta * fadeMultiplier;
			if ( currentVolume <= 0 || !sound.IsPlaying )
				break;

			sound.SetVolume( currentVolume );
			await GameTask.DelaySeconds( fadeRate );
			untilDone += Time.Delta;
		}
	}
}
