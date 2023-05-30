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
	}
}
