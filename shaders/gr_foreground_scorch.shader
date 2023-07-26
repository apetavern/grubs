
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
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( BlendMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( ScorchColour, Srgb, 8, "None", "_color", "Scorch,10/,0/1", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchBlendMask, Linear, 8, "None", "_mask", "Scorch,10/,0/6", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Textures,3/,0/1", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchNormal, Linear, 8, "NormalizeNormals", "_normal", "Scorch,10/,0/3", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", "Textures,3/,0/2", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchRough, Linear, 8, "None", "_rough", "Scorch,10/,0/4", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( AO, Linear, 8, "None", "_ao", "Textures,3/,0/3", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchAO, Linear, 8, "None", "_ao", "Scorch,10/,0/5", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	Texture2D g_tColour < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBlendMask < Channel( RGBA, Box( BlendMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tScorchColour < Channel( RGBA, Box( ScorchColour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tScorchLayer < Attribute( "ScorchLayer" ); >;
	Texture2D g_tScorchBlendMask < Channel( RGBA, Box( ScorchBlendMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tScorchNormal < Channel( RGBA, Box( ScorchNormal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tRough < Channel( RGBA, Box( Rough ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tScorchRough < Channel( RGBA, Box( ScorchRough ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tAO < Channel( RGBA, Box( AO ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tScorchAO < Channel( RGBA, Box( ScorchAO ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float4 g_vTint_Colour < UiType( Color ); UiGroup( "Tint,2/,0/2" ); Default4( 0.41, 0.22, 0.11, 1.00 ); >;
	bool g_bTintDirectionToggle < UiGroup( "Tint,1/,0/1" ); Default( 0 ); >;
	bool g_bGradientTint < UiGroup( "Tint,0/,0/0" ); Default( 1 ); >;
	float4 g_vScorchTint_Colour < UiType( Color ); UiGroup( "Scorch,10/,0/2" ); Default4( 0.13, 0.13, 0.12, 1.00 ); >;
	float4 g_vScorchLayer_Params < Attribute( "ScorchLayer_Params" ); >;
		
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
		float4 l_5 = saturate( lerp( l_3, SoftLight_blend( l_3, l_2 ), l_4.a ) );
		float3 l_6 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float l_7 = l_6.z;
		float l_8 = l_7 + 1;
		float l_9 = l_8 / 2000;
		float l_10 = saturate( l_9 );
		float l_11 = 1 - l_10;
		float l_12 = g_bTintDirectionToggle ? l_10 : l_11;
		float4 l_13 = saturate( lerp( l_2, Overlay_blend( l_2, l_5 ), l_12 ) );
		float4 l_14 = saturate( lerp( l_3, l_2, 0.5 ) );
		float4 l_15 = g_bGradientTint ? l_13 : l_14;
		float4 l_16 = g_vScorchTint_Colour;
		float4 l_17 = Tex2DS( g_tScorchColour, g_sSampler0, l_1 );
		float4 l_18 = l_16 * l_17;
		float4 l_19 = g_vScorchLayer_Params;
		float4 l_20 = float4( l_19.r, l_19.r, 0, 0 );
		float3 l_21 = i.vPositionOs;
		float3 l_22 = float3( l_19.b, l_19.b, l_19.b ) * l_21;
		float4 l_23 = l_20 + float4( l_22, 0 );
		float4 l_24 = Tex2DS( g_tScorchLayer, g_sSampler1, l_23.xy );
		float l_25 = l_24.r - 0.5;
		float l_26 = l_25 * l_19.a;
		float l_27 = l_19.a / 1;
		float l_28 = l_26 * l_27;
		float l_29 = l_28 * -0.5;
		float4 l_30 = Tex2DS( g_tScorchBlendMask, g_sSampler0, l_1 );
		float l_31 = l_30.r - 0.5;
		float l_32 = l_31 * 32;
		float l_33 = l_29 - l_32;
		float l_34 = max( l_33, 0 );
		float l_35 = min( l_34, 1 );
		float4 l_36 = lerp( l_15, l_18, l_35 );
		float4 l_37 = Tex2DS( g_tNormal, g_sSampler0, l_1 );
		float4 l_38 = Tex2DS( g_tScorchNormal, g_sSampler0, l_1 );
		float4 l_39 = lerp( l_37, l_38, l_35 );
		float3 l_40 = TransformNormal( i, DecodeNormal( l_39.xyz ) );
		float4 l_41 = Tex2DS( g_tRough, g_sSampler0, l_1 );
		float4 l_42 = Tex2DS( g_tScorchRough, g_sSampler0, l_1 );
		float l_43 = lerp( l_41.a, l_42.r, l_35 );
		float4 l_44 = Tex2DS( g_tAO, g_sSampler0, l_1 );
		float4 l_45 = Tex2DS( g_tScorchAO, g_sSampler0, l_1 );
		float l_46 = lerp( l_44.r, l_45.r, l_35 );
		
		m.Albedo = l_36.xyz;
		m.Opacity = 1;
		m.Normal = l_40;
		m.Roughness = l_43;
		m.Metalness = 0;
		m.AmbientOcclusion = l_46;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		return ShadingModelStandard::Shade( i, m );
	}
}
