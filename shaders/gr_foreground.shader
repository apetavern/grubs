
HEADER
{
	Description = "";
}

FEATURES
{
    #include "common/features.hlsl"
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
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", "Main,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Main,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", "Main,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateTexture2DWithoutSampler( g_tColour ) < Channel( RGBA, Box( Colour ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tRough ) < Channel( RGBA, Box( Rough ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;

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
		float4 local1 = Tex2DS( g_tColour, g_sSampler0, local0 );
		float4 local2 = float4( 0.24855489, 0.07742485, 0, 1 );
		float4 local3 = saturate( lerp( local2, Overlay_blend( local2, local1 ), 1 ) );
		float local4 = ( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ).y;
		float local5 = local4 + 20;
		float local6 = local5 / 75;
		float local7 = saturate( local6 );
		float4 local8 = lerp( local1, local3, local7 );
		float local9 = ( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ).z;
		float local10 = local9 + 1024;
		float local11 = local10 / 2000;
		float local12 = saturate( local11 );
		float4 local13 = lerp( local3, local1, local12 );
		float4 local14 = lerp( local8, local13, 0.65 );
		float4 local15 = Tex2DS( g_tNormal, g_sSampler0, local0 );
		float3 local16 = TransformNormal( i, DecodeNormal( local15.xyz ) );
		float4 local17 = Tex2DS( g_tRough, g_sSampler0, local0 );
		float local18 = 1;
		float local19 = 0.5;
		float local20 = lerp( local18, local19, local13.x );
		float local21 = saturate( local20 );

		m.Albedo = local14.xyz;
		m.Normal = local16;
		m.Roughness = local17.x;
		m.AmbientOcclusion = local21;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
