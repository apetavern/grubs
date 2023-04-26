
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

		float4 local0 = g_vColor;
		float local1 = g_flEmissionStrength;
		float4 local2 = local0 * float4( local1, local1, local1, local1 );
		float local3 = g_flSmoothStepMin;
		float local4 = g_flSmoothStepMax;
		float2 local5 = PolarCoordinates( ( i.vTextureCoords.xy ) - ( float2( 0.5, 0.5 ) ), 3, 1 );
		float local6 = g_flSpeedOne;
		float local7 = local6 * g_flTime;
		float local8 = 1 - local7;
		float2 local9 = local5 + float2( local8, local8 );
		float4 local10 = Tex2DS( g_tTexture, g_sSampler0, local9 );
		float2 local11 = PolarCoordinates( ( i.vTextureCoords.xy ) - ( float2( 0.5, 0.5 ) ), 3, 1 );
		float local12 = g_flSpeedTwo;
		float local13 = local12 * g_flTime;
		float local14 = 1 - local13;
		float2 local15 = local11 + float2( local14, local14 );
		float4 local16 = Tex2DS( g_tTexture0, g_sSampler0, local15 );
		float3 local17 = i.vVertexColor.rgb;
		float local18 = local17.x;
		float local19 = local18 * 5;
		float4 local20 = lerp( local10, local16, local19 );
		float4 local21 = smoothstep( local3, local4, local20 );

		m.Albedo = local0.xyz;
		m.Emission = local2.xyz;
		m.Opacity = local21.x;
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
