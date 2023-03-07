
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
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", "Textures,3/,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateTexture2DWithoutSampler( g_tColour ) < Channel( RGBA, Box( Colour ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tRough ) < Channel( RGBA, Box( Rough ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float g_flTiling < UiGroup( "Textures,0/,1/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float4 g_vTintColour < UiType( Color ); UiGroup( "Tint,0/,0/0" ); Default4( 0.25, 0.08, 0.00, 1.00 ); >;
	float g_flYPosition < UiGroup( "Position,0/Y,0/0" ); Default1( 20 ); Range1( 0, 64 ); >;
	float g_flYSmoothing < UiGroup( "Position,0/Y,0/0" ); Default1( 75 ); Range1( 0, 128 ); >;
	float g_flZPosition < UiGroup( "Position,0/Z,1/0" ); Default1( 1024 ); Range1( 0, 2048 ); >;
	float g_flZSmoothing < UiGroup( "Position,0/Z,1/0" ); Default1( 2000 ); Range1( 0, 2048 ); >;
	float g_flTintBlend < UiGroup( "Tint,0/,0/0" ); Default1( 0.65 ); Range1( 0, 1 ); >;
	float g_flAOPlus < UiGroup( "AO,2/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flAOMinus < UiGroup( "AO,2/,1/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;

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
		float4 local5 = saturate( lerp( local4, Overlay_blend( local4, local3 ), 1 ) );
		float3 local6 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float local7 = local6.y;
		float local8 = g_flYPosition;
		float local9 = local7 + local8;
		float local10 = g_flYSmoothing;
		float local11 = local9 / local10;
		float local12 = saturate( local11 );
		float4 local13 = lerp( local3, local5, local12 );
		float local14 = local6.z;
		float local15 = g_flZPosition;
		float local16 = local14 + local15;
		float local17 = g_flZSmoothing;
		float local18 = local16 / local17;
		float local19 = saturate( local18 );
		float4 local20 = lerp( local5, local3, local19 );
		float local21 = g_flTintBlend;
		float4 local22 = lerp( local13, local20, local21 );
		float4 local23 = Tex2DS( g_tNormal, g_sSampler0, local2 );
		float3 local24 = TransformNormal( i, DecodeNormal( local23.xyz ) );
		float4 local25 = Tex2DS( g_tRough, g_sSampler0, local2 );
		float local26 = g_flAOPlus;
		float local27 = g_flAOMinus;
		float local28 = lerp( local26, local27, local20.x );
		float local29 = saturate( local28 );

		m.Albedo = local22.xyz;
		m.Opacity = 1;
		m.Normal = local24;
		m.Roughness = local25.x;
		m.Metalness = 0;
		m.AmbientOcclusion = local29;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
