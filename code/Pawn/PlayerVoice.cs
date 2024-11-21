using Sandbox.Audio;

namespace Grubs.Pawn;

[Title( "Grubs - Player Voice" ), Category( "Grubs" )]
public sealed class PlayerVoice : Voice
{
	private List<Connection> _blockedConnections = new();

	protected override IEnumerable<Connection> ExcludeFilter() => _blockedConnections;

	protected override void OnStart()
	{
		base.OnStart();

		TargetMixer = Mixer.FindMixerByName( "Voice" );
	}

	public void ToggleBlock( Player player )
	{
		var connection = player.Network.Owner;

		// Unblocking.
		if ( _blockedConnections.Contains( connection ) )
		{
			_blockedConnections.Remove( connection );
			player.Voice.Volume = 1f;
			return;
		}

		// Blocking.
		_blockedConnections.Add( connection );
		player.Voice.Volume = 0f;
	}
}
