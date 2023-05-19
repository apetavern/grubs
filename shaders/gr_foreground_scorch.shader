
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
	float3 vPositionOs : TEXCOORD14;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		o.vPositionOs = i.vPositionOs.xyz;
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
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( BlendMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( ScorchColour, Srgb, 8, "None", "_color", "Scorch,10/,0/1", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchBlendMask, Linear, 8, "None", "_mask", "Scorch,10/,0/6", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchNormal, Linear, 8, "NormalizeNormals", "_normal", "Scorch,10/,0/3", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ScorchRough, Linear, 8, "None", "_rough", "Scorch,10/,0/4", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( AO, Linear, 8, "None", "_ao", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
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
	float g_flTiling < UiGroup( "Textures,0/,1/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float4 g_vTint_Colour < UiType( Color ); UiGroup( "Tint,0/,0/0" ); Default4( 0.41, 0.22, 0.11, 1.00 ); >;
	float g_flYPosition < UiGroup( "Position,0/Y,0/3" ); Default1( 64 ); Range1( 0, 1024 ); >;
	float g_flYSmoothing < UiGroup( "Position,0/Y,0/4" ); Default1( 75 ); Range1( 0, 1024 ); >;
	float g_flZPosition < UiGroup( "Position,0/Z,1/1" ); Default1( 0 ); Range1( 0, 2048 ); >;
	float g_flZSmoothing < UiGroup( "Position,0/Z,1/2" ); Default1( 250 ); Range1( 0, 2048 ); >;
	bool g_bTintDirectionToggle < UiGroup( "Tint,1/,0/0" ); Default( 0 ); >;
	float4 g_vScorchTint_Colour < UiType( Color ); UiGroup( "Scorch,10/,0/2" ); Default4( 0.13, 0.13, 0.12, 1.00 ); >;
	float4 g_vScorchLayer_Params < Attribute( "ScorchLayer_Params" ); >;
	float g_flScorchBlendDistance < UiGroup( "Scorch,10/,0/10" ); Default1( 32 ); Range1( 0, 256 ); >;

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
		float4 local4 = g_vTint_Colour;
		float4 local5 = Tex2DS( g_tBlendMask, g_sSampler0, local2 );
		float4 local6 = saturate( lerp( local4, SoftLight_blend( local4, local3 ), local5.a ) );
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
		float local21 = local20 * local4.a;
		float local22 = 1 - local21;
		float local23 = g_bTintDirectionToggle ? local21 : local22;
		float4 local24 = saturate( lerp( local3, Overlay_blend( local3, local6 ), local23 ) );
		float4 local25 = local14 * local24;
		float4 local26 = g_vScorchTint_Colour;
		float4 local27 = Tex2DS( g_tScorchColour, g_sSampler0, local2 );
		float4 local28 = local26 * local27;
		float4 local29 = g_vScorchLayer_Params;
		float4 local30 = float4( local29.r, local29.g, 0, 0 );
		float3 local31 = i.vPositionOs;
		float3 local32 = float3( local29.b, local29.b, local29.b ) * local31;
		float4 local33 = local30 + float4( local32, 0 );
		float4 local34 = Tex2DS( g_tScorchLayer, g_sSampler1, local33.xy );
		float local35 = local34.r - 0.5;
		float local36 = local35 * local29.a;
		float local37 = g_flScorchBlendDistance;
		float local38 = local29.a / local37;
		float local39 = local36 * local38;
		float local40 = local39 * -0.5;
		float4 local41 = Tex2DS( g_tScorchBlendMask, g_sSampler0, local2 );
		float local42 = local41.r - 0.5;
		float local43 = local42 * 32;
		float local44 = local40 - local43;
		float local45 = max( local44, 0 );
		float local46 = min( local45, 1 );
		float4 local47 = lerp( local25, local28, local46 );
		float4 local48 = Tex2DS( g_tNormal, g_sSampler0, local2 );
		float4 local49 = Tex2DS( g_tScorchNormal, g_sSampler0, local2 );
		float4 local50 = lerp( local48, local49, local46 );
		float3 local51 = TransformNormal( i, DecodeNormal( local50.xyz ) );
		float4 local52 = Tex2DS( g_tRough, g_sSampler0, local2 );
		float4 local53 = Tex2DS( g_tScorchRough, g_sSampler0, local2 );
		float local54 = lerp( local52.a, local53.r, local46 );
		float4 local55 = Tex2DS( g_tAO, g_sSampler0, local2 );
		float4 local56 = Tex2DS( g_tScorchAO, g_sSampler0, local2 );
		float local57 = lerp( local55.r, local56.r, local46 );

		m.Albedo = local47.xyz;
		m.Opacity = 1;
		m.Normal = local51;
		m.Roughness = local54;
		m.Metalness = 0;
		m.AmbientOcclusion = local57;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
