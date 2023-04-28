
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
	Texture2D g_tTexture < Channel( RGBA, Box( Texture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTexture0 < Channel( RGBA, Box( Texture0 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float4 g_vColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flEmissionStrength < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 10 ); >;
	float g_flSmoothStepMin < UiGroup( ",0/,0/0" ); Default1( 0.25 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMax < UiGroup( ",0/,0/1" ); Default1( 0.35 ); Range1( 0, 1 ); >;
	float2 g_vUVTilingOne < UiGroup( ",0/,0/0" ); Default2( 10,1 ); >;
	float g_flSpeedOne < UiGroup( ",0/,0/0" ); Default1( 0.1 ); Range1( 0, 1 ); >;
	float2 g_vUVTilingTwo < UiGroup( ",0/,0/0" ); Default2( 16,1 ); >;
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

		float4 local0 = g_vColor;
		float local1 = g_flEmissionStrength;
		float4 local2 = local0 * float4( local1, local1, local1, local1 );
		float local3 = g_flSmoothStepMin;
		float local4 = g_flSmoothStepMax;
		float2 local5 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 local6 = g_vUVTilingOne;
		float local7 = g_flSpeedOne;
		float local8 = local7 * g_flTime;
		float local9 = 1 - local8;
		float2 local10 = TileAndOffsetUv( local5, local6, float2( local9, local9 ) );
		float4 local11 = Tex2DS( g_tTexture, g_sSampler0, local10 );
		float2 local12 = g_vUVTilingTwo;
		float local13 = g_flSpeedTwo;
		float local14 = local13 * g_flTime;
		float local15 = 1 - local14;
		float2 local16 = TileAndOffsetUv( local5, local12, float2( local15, local15 ) );
		float4 local17 = Tex2DS( g_tTexture0, g_sSampler0, local16 );
		float4 local18 = lerp( local11, local17, 0.5 );
		float4 local19 = smoothstep( local3, local4, local18 );

		m.Albedo = local0.xyz;
		m.Emission = local2.xyz;
		m.Opacity = local19.x;
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
