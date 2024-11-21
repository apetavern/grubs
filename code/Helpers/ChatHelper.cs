using Grubs.Pawn;

namespace Grubs.Helpers;

[Title( "Grubs - Chat Helper" ), Category( "Networking" )]
public sealed class ChatHelper : Component
{
	public struct ChatMessage
	{
		public string AuthorName { get; init; }
		public ulong AuthorSteamId { get; init; }
		public string Message { get; init; }
		public string Color { get; init; }
		public TimeSince Lifetime { get; init; }
	}

	public static ChatHelper Instance { get; private set; }

	public List<ChatMessage> Messages { get; } = new List<ChatMessage>();

	public delegate void MessageReceived( ChatMessage message );
	public event MessageReceived OnMessageReceived;

	public ChatHelper()
	{
		Instance = this;
	}

	[Broadcast]
	public void SendMessage( string messageText )
	{
		if ( messageText.Contains( '\n' ) || messageText.Contains( '\r' ) )
			return;

		var player = Scene.GetAllComponents<Player>().FirstOrDefault( x => x.Network.Owner == Rpc.Caller );
		var message = new ChatMessage()
		{
			AuthorName = Rpc.Caller.DisplayName,
			AuthorSteamId = Rpc.Caller.SteamId,
			Message = messageText,
			Color = player?.SelectedColor ?? Color.White.Hex,
			Lifetime = 0f
		};

		Messages.Add( message );
		OnMessageReceived?.Invoke( message );
	}

	[Broadcast]
	public void SendInfoMessage( string messageText )
	{
		if ( messageText.Contains( '\n' ) || messageText.Contains( '\r' ) )
			return;

		var message = new ChatMessage()
		{
			AuthorName = string.Empty,
			AuthorSteamId = 0,
			Message = messageText,
			Color = Color.White.Hex,
			Lifetime = 0f
		};

		Messages.Add( message );
		OnMessageReceived?.Invoke( message );
	}
}
