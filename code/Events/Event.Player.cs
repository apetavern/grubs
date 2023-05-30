namespace Grubs;

public static partial class GrubsEvent
{
	public static class Player
	{
		public const string ChatMessageSent = "player.chat.message";

		/// <summary>
		/// Called when a chat message is sent by a player.
		/// <para><see cref="UI.TextChat.ChatMessage"/>A struct containing the details of the chat message.</para>
		/// </summary>
		public class ChatMessageSentAttribute : EventAttribute
		{
			public ChatMessageSentAttribute() : base( ChatMessageSent ) { }
		}

		public const string Disconnect = "player.disconnect";

		/// <summary>
		/// Called when a player disconnects from the game. This is ran BEFORE <see cref="Gamemode.OnClientDisconnect"/>
		/// (before they are added to <see cref="Gamemode.DisconnectedPlayers"/>).
		/// <para><see cref="Grubs.Player"/>The player who is disconnecting.</para>
		/// </summary>
		public class PlayerDisconnectAttribute : EventAttribute
		{
			public PlayerDisconnectAttribute() : base( Disconnect ) { }
		}
	}
}
