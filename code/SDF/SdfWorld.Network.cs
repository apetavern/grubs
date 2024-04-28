using Grubs.Terrain;

namespace Sandbox.Sdf;

public partial class SdfWorld<TWorld, TChunk, TResource, TChunkKey, TArray, TSdf> : Component
{
	private Dictionary<Connection, ConnectionState> ConnectionStates { get; } = new();

	private record struct ConnectionState( int clearCount, int modificationCount, TimeSince lastMessage );

	private const float HeartbeatPeriod = 2f;

	private void SendModifications( Connection conn )
	{
		if ( !ConnectionStates.TryGetValue( conn, out var state ) )
			state = new ConnectionState( 0, 0, 0f );

		if ( state.clearCount != ClearCount )
			state = state with { clearCount = ClearCount, modificationCount = 0 };
		else if ( state.modificationCount >= ModificationCount && state.lastMessage < HeartbeatPeriod )
			return;

		state = state with { lastMessage = 0f };

		var byteStream = ByteStream.Create( 512 );
		var count = Write( ref byteStream, state.modificationCount );

		ConnectionStates[conn] = state with { modificationCount = state.modificationCount + count };

		GrubsTerrain.Instance.WriteRpc( conn.Id, byteStream.ToArray() );
		byteStream.Dispose();
	}

	public void RequestMissing( Connection conn, int clearCount, int modificationCount )
	{
		if ( !ConnectionStates.TryGetValue( conn, out var state ) )
		{
			Log.Info( $"Can't find connection state for {conn.DisplayName}" );
			return;
		}

		if ( state.clearCount != clearCount || state.modificationCount <= modificationCount )
		{
			Log.Info( $"Can't do something else" );
			return;
		}


		ConnectionStates[conn] = state with { modificationCount = modificationCount };
	}
}
