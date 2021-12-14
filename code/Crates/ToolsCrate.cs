using Sandbox;

namespace TerryForm.Crates
{
	[Library( "crate_tools" )]
	public class ToolsCrate : BaseCrate
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/tools_crate/tools_crate.vmdl" );
		}
	}
}
