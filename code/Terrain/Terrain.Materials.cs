using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	public Sdf2DMaterial DevMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf2d_default.sdflayer" );
	public Sdf2DMaterial SandMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/sand.sdflayer" );
	public Sdf2DMaterial RockMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/rock.sdflayer" );

	private Dictionary<Sdf2DMaterial, float> GetSandMaterials(
		bool includeForeground = true,
		bool includeBackground = false,
		float fgOffset = 0,
		float bgOffset = 0 )
	{
		var sandMaterials = new Dictionary<Sdf2DMaterial, float>();
		if ( includeForeground )
			sandMaterials.Add( SandMaterial, fgOffset );
		if ( includeBackground )
			sandMaterials.Add( RockMaterial, bgOffset );

		return sandMaterials;
	}
}
