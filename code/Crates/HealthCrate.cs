using Sandbox;

namespace TerryForm.Crates
{
	[Library( "crate_health" )]
	public class HealthCrate : BaseCrate
	{
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/crates/health_crate/health_crate.vmdl" );
		}
	}
}
