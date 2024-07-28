
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
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
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
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( WaterNoise, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( DistortNoise, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tWaterNoise < Channel( RGBA, Box( WaterNoise ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tDistortNoise < Channel( RGBA, Box( DistortNoise ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float4 g_vWaterColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.03, 0.21, 0.50, 1.00 ); >;
	float4 g_vFoamColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.64, 0.83, 0.93, 1.00 ); >;
	float2 g_vDistortTiling < UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( 0,0, 1,1 ); >;
	float g_flScrollSpeed < UiGroup( ",0/,0/0" ); Default1( 0.01 ); Range1( 0, 1 ); >;
	float g_flDistortAmount < UiGroup( ",0/,0/0" ); Default1( 0.15 ); Range1( 0, 1 ); >;
	float2 g_vTiling < UiGroup( ",0/,0/0" ); Default2( 2,2 ); Range2( 0,0, 1,1 ); >;
	float g_flNoiseStrength < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0, 1 ); >;
	float g_flRoughness < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0, 1 ); >;
	
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
		
		float4 l_0 = g_vWaterColour;
		float4 l_1 = g_vFoamColour;
		float2 l_2 = g_vDistortTiling;
		float l_3 = g_flScrollSpeed;
		float l_4 = g_flTime * l_3;
		float2 l_5 = TileAndOffsetUv( i.vTextureCoords.xy, l_2, float2( l_4, l_4 ) );
		float4 l_6 = Tex2DS( g_tDistortNoise, g_sSampler0, l_5 );
		float l_7 = g_flDistortAmount;
		float2 l_8 = lerp( l_5, float2( l_6.r, l_6.r ), l_7 );
		float2 l_9 = g_vTiling;
		float l_10 = g_flScrollSpeed;
		float l_11 = g_flTime * l_10;
		float2 l_12 = TileAndOffsetUv( l_8, l_9, float2( l_11, l_11 ) );
		float4 l_13 = Tex2DS( g_tWaterNoise, g_sSampler0, l_12 );
		float l_14 = g_flNoiseStrength;
		float l_15 = l_13.g * l_14;
		float4 l_16 = lerp( l_0, l_1, l_15 );
		float l_17 = g_flRoughness;
		
		m.Albedo = l_16.xyz;
		m.Opacity = 1;
		m.Roughness = l_17;
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
