
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
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Gradient, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( NoiseOne, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( NoiseTwo, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( DistortionMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( FlameMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tGradient < Channel( RGBA, Box( Gradient ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNoiseOne < Channel( RGBA, Box( NoiseOne ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNoiseTwo < Channel( RGBA, Box( NoiseTwo ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tDistortionMask < Channel( RGBA, Box( DistortionMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tFlameMask < Channel( RGBA, Box( FlameMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float4 g_vColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m;
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = TransformNormal( i, float3( 0, 0, 1 ) );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_1 = l_0.x;
		float l_2 = 0 * g_flTime;
		float l_3 = l_0.y;
		float l_4 = l_2 + l_3;
		float4 l_5 = float4( l_1, l_4, 0, 0 );
		float4 l_6 = Tex2DS( g_tNoiseOne, g_sSampler1, l_5.xy );
		float2 l_7 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_8 = l_7.x;
		float l_9 = 0 * g_flTime;
		float l_10 = l_7.y;
		float l_11 = l_9 + l_10;
		float4 l_12 = float4( l_8, l_11, 0, 0 );
		float4 l_13 = Tex2DS( g_tNoiseTwo, g_sSampler1, l_12.xy );
		float4 l_14 = l_6 + l_13;
		float4 l_15 = Tex2DS( g_tDistortionMask, g_sSampler1, i.vTextureCoords.xy );
		float4 l_16 = l_14 * l_15;
		float4 l_17 = l_16 * float4( 1, 1, 1, 1 );
		float2 l_18 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_19 = l_17 + float4( l_18, 0, 0 );
		float4 l_20 = Tex2DS( g_tGradient, g_sSampler0, l_19.xy );
		float4 l_21 = l_14 * float4( 1, 1, 1, 1 );
		float2 l_22 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_23 = l_21 + float4( l_22, 0, 0 );
		float4 l_24 = Tex2DS( g_tFlameMask, g_sSampler1, l_23.xy );
		float4 l_25 = l_20 * l_24;
		float4 l_26 = g_vColour;
		float4 l_27 = l_25 * l_26;
		float4 l_28 = l_27 * float4( 1, 1, 1, 1 );
		float4 l_29 = smoothstep( 0.0f, 0.0f, l_25 );
		float4 l_30 = saturate( l_29 );
		
		m.Albedo = l_27.xyz;
		m.Emission = l_28.xyz;
		m.Opacity = l_30.x;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		return ShadingModelStandard::Shade( i, m );
	}
}
