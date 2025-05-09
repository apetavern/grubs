
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
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( SkyTexture, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( SkyTexture_0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tSkyTexture < Channel( RGBA, Box( SkyTexture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tSkyTexture_0 < Channel( RGBA, Box( SkyTexture_0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tSkyTexture_0 )
	TextureAttribute( RepresentativeTexture, g_tSkyTexture_0 )
	float3 g_vWind < UiGroup( ",0/,0/0" ); Default3( 1,1,0 ); Range3( 0,0,0, 1,1,1 ); >;
	float g_flFlowSpeed < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;
	float g_flFlowStrength < UiGroup( ",0/,0/0" ); Default1( 0.25 ); Range1( 0, 1 ); >;
	
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
		
		float3 l_0 = float3( 0, -1, 0 );
		float4 l_1 = float4( l_0.x, l_0.y, l_0.z, 0 );
		float3 l_2 = g_vWind;
		float4 l_3 = l_1 * float4( l_2, 0 );
		float3 l_4 = Vec3WsToTs( DecodeNormal( l_3.xyz ), i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		float l_5 = l_4.x;
		float l_6 = l_4.y;
		float4 l_7 = float4( l_5, l_6, 0, 0 );
		float2 l_8 = float2( 1, -1 );
		float4 l_9 = l_7 * float4( l_8, 0, 0 );
		float l_10 = g_flFlowSpeed;
		float4 l_11 = l_9 * float4( l_10, l_10, l_10, l_10 );
		float3 l_12 = l_11.xyz;
		float l_13 = l_12.x;
		float l_14 = l_12.y;
		float4 l_15 = float4( l_13, l_14, 0, 0 );
		float l_16 = g_flFlowStrength;
		float4 l_17 = l_15 * float4( l_16, l_16, l_16, l_16 );
		float l_18 = g_flTime * 0.1;
		float l_19 = frac( l_18 );
		float4 l_20 = l_17 * float4( l_19, l_19, l_19, l_19 );
		float2 l_21 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_22 = l_20 + float4( l_21, 0, 0 );
		float4 l_23 = Tex2DS( g_tSkyTexture, g_sSampler0, l_22.xy );
		float l_24 = l_18 + 0.5;
		float l_25 = frac( l_24 );
		float4 l_26 = l_17 * float4( l_25, l_25, l_25, l_25 );
		float4 l_27 = l_26 + float4( l_21, 0, 0 );
		float4 l_28 = Tex2DS( g_tSkyTexture_0, g_sSampler0, l_27.xy );
		float l_29 = l_19 * 2;
		float l_30 = l_29 - 1;
		float l_31 = abs( l_30 );
		float4 l_32 = lerp( l_23, l_28, l_31 );
		float4 l_33 = saturate( l_32 );
		
		m.Albedo = l_32.xyz;
		m.Emission = l_33.xyz;
		m.Opacity = 1;
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
