
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	float4 g_vColour < UiType( Color ); UiGroup( "Colour,1/Emission,1/0" ); Default4( 0.00, 448.08, 500.00, 1.00 ); >;
	float g_flColourStrength < UiGroup( "Colour,0/,0/0" ); Default1( 2 ); Range1( 0, 25 ); >;
	float g_flGapOpacity < UiGroup( "Adjustments,0/,0/0" ); Default1( 0.25 ); Range1( 0, 1 ); >;
	float g_flTiling < UiGroup( "Adjustments,0/Tiling,1/0" ); Default1( -0.25 ); Range1( -1, 1 ); >;
	float g_flSpeed < UiGroup( "Adjustments,0/Speed,2/0" ); Default1( 2 ); Range1( 0, 5 ); >;
	float g_flSolidOpacity < UiGroup( "Adjustments,0/,0/0" ); Default1( 0.75 ); Range1( 0, 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float4 l_0 = g_vColour;
		float l_1 = g_flColourStrength;
		float4 l_2 = l_0 * float4( l_1, l_1, l_1, l_1 );
		float l_3 = g_flGapOpacity;
		float3 l_4 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float4 l_5 = float4( l_4, 0 ).zzzw;
		float l_6 = g_flTiling;
		float l_7 = g_flSpeed;
		float l_8 = g_flTime * l_7;
		float2 l_9 = TileAndOffsetUv( l_5.xy, float2( l_6, l_6 ), float2( l_8, l_8 ) );
		float l_10 = Simplex2D( l_9 );
		float l_11 = step( 0.005, l_10 );
		float l_12 = g_flSolidOpacity;
		float l_13 = lerp( l_3, l_11, l_12 );
		
		m.Emission = l_2.xyz;
		m.Opacity = l_13;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
				
		return ShadingModelStandard::Shade( i, m );
	}
}
