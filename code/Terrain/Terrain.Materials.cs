using Sandbox.Sdf;

namespace Grubs;

public partial class Terrain
{
	public Sdf2DMaterial DevMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf2d_default.sdflayer" );
	public Sdf2DMaterial SandMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/sand.sdflayer" );
	public Sdf2DMaterial DirtMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/dirt.sdflayer" );
	public Sdf2DMaterial RockMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/rock.sdflayer" );

	public Sdf2DMaterial GirderMaterial { get; } = ResourceLibrary.Get<Sdf2DMaterial>( "materials/sdf/girder.sdflayer" );

	public Dictionary<Sdf2DMaterial, float> GetActiveMaterials( MaterialsConfig cfg )
	{
		var materials = new Dictionary<Sdf2DMaterial, float>();

		var terrainType = GrubsConfig.WorldTerrainEnvironmentType;

		List<Sdf2DMaterial> activeMaterials;

		activeMaterials = terrainType switch
		{
			GrubsConfig.TerrainEnvironmentType.Sand => GetSandMaterials(),
			GrubsConfig.TerrainEnvironmentType.Dirt => GetDirtMaterials(),
			_ => GetSandMaterials(),
		};

		if ( cfg.includeForeground )
			materials.Add( activeMaterials.ElementAt( 0 ), cfg.fgOffset );
		if ( cfg.includeBackground )
			materials.Add( activeMaterials.ElementAt( 1 ), cfg.bgOffset );
		if ( cfg.isDestruction )
			materials.Add( GetAllMaterials().First(), 0f );
		
		return materials;
	}

	public List<Sdf2DMaterial> GetSandMaterials()
	{
		return new List<Sdf2DMaterial>() { SandMaterial, RockMaterial };
	}

	public List<Sdf2DMaterial> GetDirtMaterials()
	{
		return new List<Sdf2DMaterial>() { DirtMaterial, RockMaterial };
	}

	public List<Sdf2DMaterial> GetGirderMaterials()
	{
		return new List<Sdf2DMaterial>() { GirderMaterial };
	}

	public List<Sdf2DMaterial> GetAllMaterials()
	{
		return new List<Sdf2DMaterial>() { GirderMaterial };
	}
}

public struct MaterialsConfig
{
	public bool includeForeground = true;
	public bool includeBackground = false;
	public bool isDestruction = true;
	public float fgOffset = 0;
	public float bgOffset = 0;

	public MaterialsConfig(
		bool includeForeground = true,
		bool includeBackground = false,
		bool isDestruction = true,
		float fgOffset = 0,
		float bgOffset = 0 )
	{
		this.includeForeground = includeForeground;
		this.includeBackground = includeBackground;
		this.isDestruction = isDestruction;
		this.fgOffset = fgOffset;
		this.bgOffset = bgOffset;
	}

	public static MaterialsConfig Default => new( true, false, false, 0f, 0f );
	public static MaterialsConfig Destruction => new( true, false, true, 0f, 0f );
	public static MaterialsConfig DestructionWithBackground => new( true, true, true, 0f, 0f );

	public override string ToString()
	{
		return $"fg: {includeForeground} - {fgOffset} // bg: {includeBackground} - {bgOffset}";
	}
}

