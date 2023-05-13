
HEADER
{
	Description = "";
}

FEATURES
{
	#include "vr_common_features.fxc"
	Feature( F_ADDITIVE_BLEND, 0..1, "Blending" );
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

	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
	#include "sbox_pixel.fxc"
	#include "common/pixel.material.structs.hlsl"
	#include "common/pixel.lighting.hlsl"
	#include "common/pixel.shading.hlsl"
	#include "common/pixel.material.helpers.hlsl"
	#include "common/pixel.color.blending.hlsl"
	#include "common/proceedural.hlsl"

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
	float g_flTiling < UiGroup( "Textures,0/,1/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float4 g_vTintColour < UiType( Color ); UiGroup( "Tint,0/,0/0" ); Default4( 0.41, 0.22, 0.11, 1.00 ); >;
	float g_flYPosition < UiGroup( "Position,0/Y,0/3" ); Default1( 64 ); Range1( 0, 1024 ); >;
	float g_flYSmoothing < UiGroup( "Position,0/Y,0/4" ); Default1( 75 ); Range1( 0, 1024 ); >;
	float g_flZPosition < UiGroup( "Position,0/Z,1/1" ); Default1( 0 ); Range1( 0, 2048 ); >;
	float g_flZSmoothing < UiGroup( "Position,0/Z,1/2" ); Default1( 250 ); Range1( 0, 2048 ); >;
	bool g_bTintDirectionToggle < UiGroup( "Tint,1/,0/0" ); Default( 0 ); >;

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

		float2 local0 = i.vTextureCoords.xy * float2( 1, 1 );
		float local1 = g_flTiling;
		float2 local2 = local0 * float2( local1, local1 );
		float4 local3 = Tex2DS( g_tColour, g_sSampler0, local2 );
		float4 local4 = g_vTintColour;
		float4 local5 = Tex2DS( g_tBlendMask, g_sSampler0, local2 );
		float4 local6 = saturate( lerp( local4, SoftLight_blend( local4, local3 ), local5 ) );
		float3 local7 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float local8 = local7.y;
		float local9 = g_flYPosition;
		float local10 = local8 + local9;
		float local11 = g_flYSmoothing;
		float local12 = local10 / local11;
		float local13 = saturate( local12 );
		float4 local14 = lerp( local3, local6, local13 );
		float local15 = local7.z;
		float local16 = g_flZPosition;
		float local17 = local15 + local16;
		float local18 = g_flZSmoothing;
		float local19 = local17 / local18;
		float local20 = saturate( local19 );
		float local21 = local20 * 1;
		float local22 = 1 - local21;
		float local23 = g_bTintDirectionToggle ? local21 : local22;
		float4 local24 = saturate( lerp( local3, Overlay_blend( local3, local6 ), local23 ) );
		float4 local25 = local14 * local24;
		float4 local26 = Tex2DS( g_tNormal, g_sSampler0, local2 );
		float3 local27 = TransformNormal( i, DecodeNormal( local26.xyz ) );
		float4 local28 = Tex2DS( g_tRough, g_sSampler0, local2 );
		float4 local29 = Tex2DS( g_tAO, g_sSampler0, local2 );

		m.Albedo = local25.xyz;
		m.Opacity = 1;
		m.Normal = local27;
		m.Roughness = local28.x;
		m.Metalness = 0;
		m.AmbientOcclusion = local29.x;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
