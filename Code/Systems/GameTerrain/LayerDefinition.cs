using Sandbox.Sdf;

namespace Grubs.Terrain;

public class LayerDefinition
{	
	private static readonly Sdf2DLayer ScorchLayer = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/scorch.sdflayer" );

	private static readonly TextureReference<Sdf2DLayer> ScorchTextureReference = new()
	{
		TargetAttribute = "ScorchLayer", Source = ScorchLayer,
	};
	
	private static readonly List<TextureReference<Sdf2DLayer>> StableTextureReference =
	[
		ScorchTextureReference,
	];

	// todo: will this need to be created on the fly to prevent overriding?
	private Material DefaultReplaceableMaterial = Material.Load( "materials/environment/sand_shells.vmat" );
	
	public string MaterialPath { get; set; }
	public LayerShaderStrategy ShaderStrategy { get; set; }

	public LayerDefinition( string materialPath, string shaderName )
	{
		Log.Info( $"Creating new LayerDefinition for {materialPath} with {shaderName}" );
		
		MaterialPath = materialPath;

		ShaderStrategy = shaderName == "shaders/complex.shader" 
			? LayerShaderStrategy.Complex : LayerShaderStrategy.Grubs;

		_layer = CreateLayer();
	}

	private Sdf2DLayer _layer;

	public Sdf2DLayer GetLayer()
	{
		if ( _layer == null )
		{
			_layer = CreateLayer();
		}
		
		return _layer;
	}

	private Sdf2DLayer CreateLayer()
	{
		var layer = new Sdf2DLayer
		{
			Depth = 128f,
			Offset = 0f,
			TexCoordSize = 2,
			FrontFaceMaterial = DefaultReplaceableMaterial,
			BackFaceMaterial = DefaultReplaceableMaterial,
			CutFaceMaterial = DefaultReplaceableMaterial,
			EdgeStyle = EdgeStyle.Round,
			MaxSmoothAngle = 45,
			EdgeRadius = 4f,
			EdgeFaces = 8,
			ReferencedTextures = StableTextureReference
		};
		
		SetupMaterialOverrides( ref layer );

		return layer;
	}

	public void SetupMaterialOverrides( ref Sdf2DLayer layer )
	{
		// todo: load from package system for complex materials
		var material = Material.Load( MaterialPath );

		var color = ShaderStrategy.GetColorTexture( material );
		var normal = ShaderStrategy.GetNormalTexture( material );
		var roughness = ShaderStrategy.GetRoughnessTexture( material );
		var ao = ShaderStrategy.GetAmbientOcclusionTexture( material );

		layer.FrontFaceMaterial.Set( MaterialConstants.GrubsColor, color );
	}
}

public enum LayerShaderStrategy
{
	/// <summary>
	/// The material for this layer comes from Grubs' internal shader.
	/// </summary>
	Grubs,
	/// <summary>
	/// The material for this layer comes from the default "complex" shader.
	/// </summary>
	Complex
}

public static class MaterialConstants
{
	public const string ComplexColor = "g_tColor";
	public const string ComplexNormal = "g_tNormal";
	public const string ComplexRoughness = "g_tRoughness"; // no clue if this is correct
	public const string ComplexAmbientOcclusion = "g_tAmbientOcclusion";

	public const string GrubsColor = "g_tColour";
	public const string GrubsNormal = "g_tNormal";
	public const string GrubsRoughness = "g_tRough";
	public const string GrubsAmbientOcclusion = "g_tAO";
	public const string GrubsScorchTintColor = "g_vScorchTint_Colour";
}

public static class LayerShaderStrategyExtensions
{
	public static Texture GetColorTexture( this LayerShaderStrategy strategy, Material material )
	{
		return strategy switch
		{
			LayerShaderStrategy.Grubs => material.GetTexture( MaterialConstants.GrubsColor ),
			LayerShaderStrategy.Complex => material.GetTexture( MaterialConstants.ComplexColor ),
			_ => material.GetTexture( MaterialConstants.ComplexColor ),
		};
	}
	
	public static Texture GetNormalTexture( this LayerShaderStrategy strategy, Material material )
	{
		return strategy switch
		{
			LayerShaderStrategy.Grubs => material.GetTexture( MaterialConstants.GrubsNormal ),
			LayerShaderStrategy.Complex => material.GetTexture( MaterialConstants.ComplexNormal ),
			_ => material.GetTexture( MaterialConstants.ComplexNormal ),
		};
	}
	
	public static Texture GetRoughnessTexture( this LayerShaderStrategy strategy, Material material )
	{
		return strategy switch
		{
			LayerShaderStrategy.Grubs => material.GetTexture( MaterialConstants.GrubsRoughness ),
			LayerShaderStrategy.Complex => material.GetTexture( MaterialConstants.ComplexRoughness ),
			_ => material.GetTexture( MaterialConstants.ComplexRoughness ),
		};
	}
	
	public static Texture GetAmbientOcclusionTexture( this LayerShaderStrategy strategy, Material material )
	{
		return strategy switch
		{
			LayerShaderStrategy.Grubs => material.GetTexture( MaterialConstants.GrubsAmbientOcclusion ),
			LayerShaderStrategy.Complex => material.GetTexture( MaterialConstants.ComplexAmbientOcclusion ),
			_ => material.GetTexture( MaterialConstants.ComplexAmbientOcclusion ),
		};
	}
}
