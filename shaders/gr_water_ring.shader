
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
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Texture_ps_0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Texture_ps_1, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tTexture_ps_0 < Channel( RGBA, Box( Texture_ps_0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTexture_ps_1 < Channel( RGBA, Box( Texture_ps_1 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float4 g_vColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float2 g_vUVTilingOne < UiGroup( ",0/,0/0" ); Default2( 10,1 ); >;
	float2 g_vUVTilingTwo < UiGroup( ",0/,0/0" ); Default2( 16,1 ); >;
	
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
		
		float4 l_0 = g_vColor;
		float4 l_1 = l_0 * float4( 2, 2, 2, 2 );
		float2 l_2 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_3 = g_vUVTilingOne;
		float l_4 = -0.3 * g_flTime;
		float l_5 = 1 - l_4;
		float2 l_6 = TileAndOffsetUv( l_2, l_3, float2( l_5, l_5 ) );
		float4 l_7 = Tex2DS( g_tTexture_ps_0, g_sSampler0, l_6 );
		float2 l_8 = g_vUVTilingTwo;
		float l_9 = -0.3 * g_flTime;
		float l_10 = 1 - l_9;
		float2 l_11 = TileAndOffsetUv( l_2, l_8, float2( l_10, l_10 ) );
		float4 l_12 = Tex2DS( g_tTexture_ps_1, g_sSampler0, l_11 );
		float4 l_13 = lerp( l_7, l_12, 0.5 );
		float4 l_14 = smoothstep( 0.0f, 0.0f, l_13 );
		
		m.Albedo = l_0.xyz;
		m.Emission = l_1.xyz;
		m.Opacity = l_14.x;
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
