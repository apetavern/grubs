namespace Grubs;

public static partial class GrubsEvent
{
	public static class Grub
	{
		public const string Damaged = "grub.damaged";

		/// <summary>
		/// Occurs when a grub is damaged.
		/// <para><see cref="int"/>The network ident of the damaged grub</para>
		/// </summary>
		public class DamagedAttribute : EventAttribute
		{
			public DamagedAttribute() : base( Damaged ) { }
		}
	}
}
