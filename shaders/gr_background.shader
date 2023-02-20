
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

	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
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

	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Normal, Srgb, 8, "NormalizeNormals", "_normal", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Rough, Srgb, 8, "None", "_rough", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateTexture2D( g_tColour ) < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateTexture2D( g_tNormal ) < Channel( RGBA, Box( Normal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateTexture2D( g_tRough ) < Channel( RGBA, Box( Rough ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

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
		float2 local1 = local0 * float2( 0.25, 0.25 );
		float4 local2 = Tex2D( g_tColour, local1 );
		float4 local3 = Tex2D( g_tNormal, local1 );
		float4 local4 = Tex2D( g_tRough, local1 );

		m.Albedo = local2.xyz;
		m.Normal = local3.xyz;
		m.Roughness = local4.x;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
