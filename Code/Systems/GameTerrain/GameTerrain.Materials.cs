using Sandbox.Sdf;

namespace Grubs.Terrain;

public partial class GameTerrain
{
	private Sdf2DLayer GenericMaterial { get; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/generic.sdflayer" );

	private const string ComplexColor = "g_tColor";
	private const string ComplexNormal = "g_tNormal";
	private const string ComplexRoughness = "g_tRoughness"; // no clue if this is correct
	private const string ComplexAmbientOcclusion = "g_tAmbientOcclusion";

	private const string GrubsColor = "g_tColour";
	private const string GrubsNormal = "g_tNormal";
	private const string GrubsRoughness = "g_tRough";
	private const string GrubsAmbientOcclusion = "g_tAO";
	private const string GrubsScorchTintColor = "g_vScorchTint_Colour";

	private void OverrideLayerMaterial( Sdf2DLayer layer, Material material )
	{
		var colorTex = material.GetTexture( ComplexColor );
		var normalTex = material.GetTexture( ComplexNormal );
		var roughnessTex = material.GetTexture( ComplexRoughness );
		var ambientOcclusionTex = material.GetTexture( ComplexAmbientOcclusion );
		
		SetLayerMaterialAttributes( layer, GrubsColor, colorTex );
		SetLayerMaterialAttributes( layer, GrubsNormal, normalTex );
		SetLayerMaterialAttributes( layer, GrubsRoughness, roughnessTex );
		SetLayerMaterialAttributes( layer, GrubsAmbientOcclusion, ambientOcclusionTex );
	}

	private void OverrideLayerScorchColor( Sdf2DLayer layer, Color color )
	{
		SetLayerMaterialAttributes( layer, GrubsScorchTintColor, color );
	}

	private void SetLayerMaterialAttributes( Sdf2DLayer layer, string attributeName, Texture texture )
	{
		layer.FrontFaceMaterial.Set( attributeName, texture );
		layer.BackFaceMaterial.Set( attributeName, texture );
		layer.CutFaceMaterial.Set( attributeName, texture );
	}
	
	private void SetLayerMaterialAttributes( Sdf2DLayer layer, string attributeName, Color color )
	{
		layer.FrontFaceMaterial.Set( attributeName, color );
		layer.BackFaceMaterial.Set( attributeName, color );
		layer.CutFaceMaterial.Set( attributeName, color );
	}

}
