using Sandbox;
using Grubs.Pawn;

namespace Grubs.Crates
{
	[Library( "crate_tools" )]
	public class ToolsCrate : Crate
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/tools_crate/tools_crate.vmdl" );
		}

		protected override void OnPickup( Worm worm )
		{
			base.OnPickup( worm );

			// TODO
		}
	}
}
