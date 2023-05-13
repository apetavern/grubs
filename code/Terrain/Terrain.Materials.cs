using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	public Sdf2DMaterial DevMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf2d_default.sdflayer" );
	public Sdf2DMaterial SandMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/sand.sdflayer" );
	public Sdf2DMaterial DirtMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/dirt.sdflayer" );
	public Sdf2DMaterial RockMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/rock.sdflayer" );

	public Dictionary<Sdf2DMaterial, float> GetActiveMaterials( MaterialsConfig cfg )
	{
		var materials = new Dictionary<Sdf2DMaterial, float>();

		var terrainType = GrubsConfig.WorldTerrainEnvironmentType;

		Sdf2DMaterial fgMaterial;
		Sdf2DMaterial bgMaterial;

		(fgMaterial, bgMaterial) = terrainType switch
		{
			GrubsConfig.TerrainEnvironmentType.Sand => GetSandMaterials(),
			GrubsConfig.TerrainEnvironmentType.Dirt => GetDirtMaterials(),
			_ => GetSandMaterials(),
		};

		if ( cfg.includeForeground )
			materials.Add( fgMaterial, cfg.fgOffset );
		if ( cfg.includeBackground )
			materials.Add( bgMaterial, cfg.bgOffset );

		return materials;
	}

	public (Sdf2DMaterial, Sdf2DMaterial) GetSandMaterials()
	{
		return (SandMaterial, RockMaterial);
	}

	public (Sdf2DMaterial, Sdf2DMaterial) GetDirtMaterials()
	{
		return (DirtMaterial, RockMaterial);
	}
}

public struct MaterialsConfig
{
	public bool includeForeground = true;
	public bool includeBackground = false;
	public float fgOffset = 0;
	public float bgOffset = 0;

	public MaterialsConfig( 
		bool includeForeground = true,
		bool includeBackground = false,
		float fgOffset = 0,
		float bgOffset = 0 )
	{
		this.includeForeground = includeForeground;
		this.includeBackground = includeBackground;
		this.fgOffset = fgOffset;
		this.bgOffset = bgOffset;
	}

	public static MaterialsConfig Default => new( true, false, 0f, 0f );

	public override string ToString()
	{
		return $"fg: {includeForeground} - {fgOffset} // bg: {includeBackground} - {bgOffset}";
	}
}
