namespace Grubs;

public static partial class GrubsEvent
{
	public static class Player
	{
		public const string PointerEventChanged = "player.pointer.event";

		/// <summary>
		/// Occurs if the pointer event changes.
		/// <para><see cref="bool"/>If the pointer events are enabled</para>
		/// </summary>
		public class PointerEventChangedAttribute : EventAttribute
		{
			public PointerEventChangedAttribute() : base( PointerEventChanged ) { }
		}
	}
}
