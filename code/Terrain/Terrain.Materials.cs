using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GrubsTerrain : Component
{
	public Sdf2DLayer DevMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf2d_default.sdflayer" );
	public Sdf2DLayer SandMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );
	public Sdf2DLayer DirtMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/dirt.sdflayer" );
	public Sdf2DLayer RockMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/rock.sdflayer" );
	public Sdf2DLayer ScorchMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/scorch.sdflayer" );
	public Sdf2DLayer CerealMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/cereal.sdflayer" );
	public Sdf2DLayer GirderMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/girder.sdflayer" );

	public Dictionary<Sdf2DLayer, float> GetActiveMaterials( MaterialsConfig cfg )
	{
		var materials = new Dictionary<Sdf2DLayer, float>();

		// var terrainType = GrubsConfig.WorldTerrainEnvironmentType;

		var activeMaterials = GetSandMaterials();

		// activeMaterials = terrainType switch
		// {
		// 	GrubsConfig.TerrainEnvironmentType.Sand => GetSandMaterials(),
		// 	GrubsConfig.TerrainEnvironmentType.Dirt => GetDirtMaterials(),
		// 	GrubsConfig.TerrainEnvironmentType.Cereal => GetCerealMaterials(),
		// 	_ => GetSandMaterials(),
		// };

		if ( cfg.includeForeground )
			materials.Add( activeMaterials.ElementAt( 0 ), cfg.fgOffset );
		if ( cfg.includeBackground )
			materials.Add( activeMaterials.ElementAt( 1 ), cfg.bgOffset );
		if ( cfg.isDestruction )
			materials.Add( GetAllMaterials().First(), 0f );

		return materials;
	}

	public List<Sdf2DLayer> GetSandMaterials()
	{
		return new List<Sdf2DLayer>() { SandMaterial, RockMaterial };
	}

	public List<Sdf2DLayer> GetDirtMaterials()
	{
		return new List<Sdf2DLayer>() { DirtMaterial, RockMaterial };
	}

	public List<Sdf2DLayer> GetCerealMaterials()
	{
		return new List<Sdf2DLayer>() { CerealMaterial, RockMaterial };
	}

	public List<Sdf2DLayer> GetGirderMaterials()
	{
		return new List<Sdf2DLayer>() { GirderMaterial };
	}

	public List<Sdf2DLayer> GetAllMaterials()
	{
		return new List<Sdf2DLayer>() { GirderMaterial };
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

	public static MaterialsConfig Default => new(true, false, false, 0f, 0f);
	public static MaterialsConfig Destruction => new(true, false, true, 0f, 0f);
	public static MaterialsConfig DestructionWithBackground => new(true, true, true, 0f, 0f);

	public override string ToString()
	{
		return $"fg: {includeForeground} - {fgOffset} // bg: {includeBackground} - {bgOffset}";
	}
}
