
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
	#include "common/pixel.config.hlsl"
	#include "common/pixel.material.structs.hlsl"
	#include "common/pixel.lighting.hlsl"
	#include "common/pixel.shading.hlsl"
	#include "common/pixel.material.helpers.hlsl"
	#include "common/proceedural.hlsl"

	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", "Main,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Main,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", "Main,0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateTexture2D( g_tColour ) < Channel( RGBA, Box( Colour ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateTexture2D( g_tNormal ) < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateTexture2D( g_tRough ) < Channel( RGBA, Box( Rough ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

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
		m.Transmission = 1;

		float2 local0 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local1 = Tex2D( g_tColour, local0 );
		float4 local2 = float4( 0.16184974, 0.05906085, 0.021937462, 1 );
		float4 local3 = local1 * local2;
		float local4 = ( i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs ).y;
		float local5 = local4 + 20;
		float local6 = local5 / 75;
		float local7 = saturate( local6 );
		float4 local8 = lerp( local1, local3, local7 );
		float local9 = ( i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs ).z;
		float local10 = local9 + 1024;
		float local11 = local10 / 2000;
		float local12 = saturate( local11 );
		float4 local13 = lerp( local3, local1, local12 );
		float4 local14 = lerp( local8, local13, 0.65 );
		float4 local15 = Tex2D( g_tNormal, local0 );
		float4 local16 = Tex2D( g_tRough, local0 );

		m.Albedo = local14.xyz;
		m.Normal = local15.xyz;
		m.Roughness = local16.x;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
