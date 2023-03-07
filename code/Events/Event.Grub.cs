namespace Grubs;

public static partial class GrubsEvent
{
	public static class Grub
	{
		public const string Damaged = "grub.damaged";
		public const string Healed = "grub.healed";

		/// <summary>
		/// Called when a grub is damaged.
		/// <para><see cref="int"/>The network ident of the damaged grub.</para>
		/// </summary>
		public class DamagedAttribute : EventAttribute
		{
			public DamagedAttribute() : base( Damaged ) { }
		}

		/// <summary>
		/// Called when a grub is healed.
		/// <para><see cref="int"/>The network ident of the healed grub.</para>
		/// <para><see cref="int"/>The amount the grub is being healed by.</para>
		/// </summary>
		public class HealedAttribute : EventAttribute
		{
			public HealedAttribute() : base( Healed ) { }
		}
	}
}
