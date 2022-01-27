using Sandbox;
using Grubs.Pawn;
using Grubs.Utils;

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

			CrateDropTables.ToolDropTypes tool = CrateDropTables.GetRandomToolFromCrate();

			var player = (worm.Owner as Pawn.Player);
			// player.PlayerInventory.Add( Library.Create<Tool>( tool.ToString() ) );
		}
	}
}
