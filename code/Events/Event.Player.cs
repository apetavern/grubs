namespace Grubs;

public static partial class GrubsEvent
{
	public static class Player
	{
		public const string PointerEventChanged = "player.pointer.event";

		/// <summary>
		/// Called when the Pointer Events behaviour changes for the player.
		/// <para><see cref="bool"/>Whether Pointer Events are enabled.</para>
		/// </summary>
		public class PointerEventChangedAttribute : EventAttribute
		{
			public PointerEventChangedAttribute() : base( PointerEventChanged ) { }
		}

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
