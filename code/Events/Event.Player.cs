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
	}
}
