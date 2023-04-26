
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

	float4 g_vColour < UiType( Color ); UiGroup( "Colour,1/Emission,1/0" ); Default4( 0.00, 448.08, 500.00, 1.00 ); >;
	float g_flColourStrength < UiGroup( "Colour,0/,0/0" ); Default1( 2 ); Range1( 0, 25 ); >;
	float g_flGapOpacity < UiGroup( "Adjustments,0/,0/0" ); Default1( 0.25 ); Range1( 0, 1 ); >;
	float g_flTiling < UiGroup( "Adjustments,0/Tiling,1/0" ); Default1( -0.25 ); Range1( -1, 1 ); >;
	float g_flSpeed < UiGroup( "Adjustments,0/Speed,2/0" ); Default1( 2 ); Range1( 0, 5 ); >;
	float g_flSolidOpacity < UiGroup( "Adjustments,0/,0/0" ); Default1( 0.75 ); Range1( 0, 1 ); >;

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

		float4 local0 = g_vColour;
		float local1 = g_flColourStrength;
		float4 local2 = local0 * float4( local1, local1, local1, local1 );
		float local3 = g_flGapOpacity;
		float3 local4 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float4 local5 = float4( local4.xyz, 0 ).zzzw;
		float local6 = g_flTiling;
		float local7 = g_flSpeed;
		float local8 = g_flTime * local7;
		float2 local9 = TileAndOffsetUv( local5.xy, float2( local6, local6 ), float2( local8, local8 ) );
		float local10 = Simplex2D( local9 );
		float local11 = step( 0.005, local10 );
		float local12 = g_flSolidOpacity;
		float local13 = lerp( local3, local11, local12 );

		m.Emission = local2.xyz;
		m.Opacity = local13;
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
