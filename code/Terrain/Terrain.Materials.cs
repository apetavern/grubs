using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	public Sdf2DLayer DevMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf2d_default.sdflayer" );
	public Sdf2DLayer SandMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );
	public Sdf2DLayer DirtMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/dirt.sdflayer" );
	public Sdf2DLayer RockMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/rock.sdflayer" );
	public Sdf2DLayer ScorchMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/scorch.sdflayer" );
	public Sdf2DLayer CerealMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/cereal.sdflayer" );
	public Sdf2DLayer GirderMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/girder.sdflayer" );

	private void InitializeMaterialLayer(Sdf2DLayer layer)
	{
		layer.EdgeStyle = EdgeStyle.Bevel;
		layer.EdgeRadius = 0.25f;
		layer.EdgeFaces = 3;
		layer.MaxSmoothAngle = 45;
	}

	public Dictionary<Sdf2DLayer, float> GetActiveMaterials( MaterialsConfig cfg )
	{
		var materials = new Dictionary<Sdf2DLayer, float>();
		var activeMaterials = GetSandMaterials();

		if ( cfg.includeBackground )
		{
			var bgMaterial = activeMaterials.ElementAt( 1 );
			InitializeMaterialLayer(bgMaterial);
			materials.Add( bgMaterial, cfg.bgOffset );
		}

		if ( cfg.includeForeground )
		{
			var fgMaterial = activeMaterials.ElementAt( 0 );
			InitializeMaterialLayer(fgMaterial);
			materials.Add( fgMaterial, cfg.fgOffset );
		}

		if ( cfg.isDestruction )
		{
			var destructionMaterial = GetAllMaterials().First();
			InitializeMaterialLayer(destructionMaterial);
			var offset = cfg.fgOffset;
			if ( materials.Any() )
				offset = materials.Max(x => x.Value) + 0.01f;
			materials.Add( destructionMaterial, offset );
		}

		return materials;
	}

	public List<Sdf2DLayer> GetSandMaterials()
	{
		InitializeMaterialLayer(SandMaterial);
		InitializeMaterialLayer(RockMaterial);
		return new List<Sdf2DLayer>() { SandMaterial, RockMaterial };
	}

	public List<Sdf2DLayer> GetDirtMaterials()
	{
		InitializeMaterialLayer(DirtMaterial);
		InitializeMaterialLayer(RockMaterial);
		return new List<Sdf2DLayer>() { DirtMaterial, RockMaterial };
	}

	public List<Sdf2DLayer> GetCerealMaterials()
	{
		InitializeMaterialLayer(CerealMaterial);
		InitializeMaterialLayer(RockMaterial);
		return new List<Sdf2DLayer>() { CerealMaterial, RockMaterial };
	}

	public List<Sdf2DLayer> GetGirderMaterials()
	{
		InitializeMaterialLayer(GirderMaterial);
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