
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
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( BlendMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "None", "_normal", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( AO, Linear, 8, "None", "_ao", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	Texture2D g_tColour < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBlendMask < Channel( RGBA, Box( BlendMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tRough < Channel( RGBA, Box( Rough ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tAO < Channel( RGBA, Box( AO ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float4 g_vTint_Colour < UiType( Color ); UiGroup( "Tint,0/,0/0" ); Default4( 0.41, 0.22, 0.11, 1.00 ); >;
	bool g_bTintDirectionToggle < UiGroup( "Tint,1/,0/0" ); Default( 0 ); >;
		
	float SoftLight_blend( float a, float b )
	{
	    if ( b <= 0.5f )
	        return 2.0f * a * b + a * a * ( 1.0f * 2.0f * b );
	    else 
	        return sqrt( a ) * ( 2.0f * b - 1.0f ) + 2.0f * a * (1.0f - b);
	}
	
	float3 SoftLight_blend( float3 a, float3 b )
	{
	    return float3(
	        SoftLight_blend( a.r, b.r ),
	        SoftLight_blend( a.g, b.g ),
	        SoftLight_blend( a.b, b.b )
		);
	}
	
	float4 SoftLight_blend( float4 a, float4 b, bool blendAlpha = false )
	{
	    return float4(
	        SoftLight_blend( a.rgb, b.rgb ).rgb,
	        blendAlpha ? SoftLight_blend( a.a, b.a ) : max( a.a, b.a )
	    );
	}
	
	float Overlay_blend( float a, float b )
	{
	    if ( a <= 0.5f )
	        return 2.0f * a * b;
	    else
	        return 1.0f - 2.0f * ( 1.0f - a ) * ( 1.0f - b );
	}
	
	float3 Overlay_blend( float3 a, float3 b )
	{
	    return float3(
	        Overlay_blend( a.r, b.r ),
	        Overlay_blend( a.g, b.g ),
	        Overlay_blend( a.b, b.b )
		);
	}
	
	float4 Overlay_blend( float4 a, float4 b, bool blendAlpha = false )
	{
	    return float4(
	        Overlay_blend( a.rgb, b.rgb ).rgb,
	        blendAlpha ? Overlay_blend( a.a, b.a ) : max( a.a, b.a )
	    );
	}
	
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
		float2 l_1 = l_0 * float2( 1, 1 );
		float4 l_2 = Tex2DS( g_tColour, g_sSampler0, l_1 );
		float4 l_3 = g_vTint_Colour;
		float4 l_4 = Tex2DS( g_tBlendMask, g_sSampler0, l_1 );
		float4 l_5 = saturate( lerp( l_3, SoftLight_blend( l_3, l_2 ), l_4 ) );
		float3 l_6 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float l_7 = l_6.y;
		float l_8 = l_7 + 1;
		float l_9 = l_8 / 75;
		float l_10 = saturate( l_9 );
		float4 l_11 = lerp( l_2, l_5, l_10 );
		float l_12 = l_6.z;
		float l_13 = l_12 + 1;
		float l_14 = l_13 / 2000;
		float l_15 = saturate( l_14 );
		float l_16 = l_15 * l_3.a;
		float l_17 = 1 - l_16;
		float l_18 = g_bTintDirectionToggle ? l_16 : l_17;
		float4 l_19 = saturate( lerp( l_2, Overlay_blend( l_2, l_5 ), l_18 ) );
		float4 l_20 = l_11 * l_19;
		float4 l_21 = Tex2DS( g_tNormal, g_sSampler0, l_1 );
		float3 l_22 = TransformNormal( i, DecodeNormal( l_21.xyz ) );
		float4 l_23 = Tex2DS( g_tRough, g_sSampler0, l_1 );
		float4 l_24 = Tex2DS( g_tAO, g_sSampler0, l_1 );
		
		m.Albedo = l_20.xyz;
		m.Opacity = 1;
		m.Normal = l_22;
		m.Roughness = l_23.x;
		m.Metalness = 0;
		m.AmbientOcclusion = l_24.x;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		return ShadingModelStandard::Shade( i, m );
	}
}
