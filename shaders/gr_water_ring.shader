
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
#define S_TRANSLUCENT 1
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
	CreateInputTexture2D( Texture, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Texture0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateTexture2DWithoutSampler( g_tTexture ) < Channel( RGBA, Box( Texture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tTexture0 ) < Channel( RGBA, Box( Texture0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float g_flSmoothStepMin < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMax < UiGroup( ",0/,0/1" ); Default1( 0.6 ); Range1( 0, 1 ); >;
	float g_flSpeedOne < UiGroup( ",0/,0/0" ); Default1( 0.1 ); Range1( 0, 1 ); >;
	float g_flSpeedTwo < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0, 1 ); >;

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

		float4 local0 = float4( 1, 1, 1, 1 );
		float local1 = g_flSmoothStepMin;
		float local2 = g_flSmoothStepMax;
		float2 local3 = PolarCoordinates( ( i.vTextureCoords.xy ) - ( float2( 0.5, 0.5 ) ), 3, 1 );
		float local4 = g_flSpeedOne;
		float local5 = local4 * g_flTime;
		float local6 = 1 - local5;
		float2 local7 = local3 + float2( local6, local6 );
		float4 local8 = Tex2DS( g_tTexture, g_sSampler0, local7 );
		float2 local9 = PolarCoordinates( ( i.vTextureCoords.xy ) - ( float2( 0.5, 0.5 ) ), 3, 1 );
		float local10 = g_flSpeedTwo;
		float local11 = local10 * g_flTime;
		float local12 = 1 - local11;
		float2 local13 = local9 + float2( local12, local12 );
		float4 local14 = Tex2DS( g_tTexture0, g_sSampler0, local13 );
		float local15 = lerp( local8.r, local14.g, 0.5 );
		float local16 = smoothstep( local1, local2, local15 );

		m.Albedo = local0.xyz;
		m.Opacity = local16;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
