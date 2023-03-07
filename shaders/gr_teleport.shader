
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
#define S_ALPHA_TEST 1
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

	float4 g_vColour < UiType( Color ); UiGroup( "Colour,1/Emission,1/0" ); Default4( 0.00, 448.08, 500.00, 1.00 ); >;
	float g_flTiling < UiGroup( "Adjustments,0/Tiling,1/0" ); Default1( -0.2 ); Range1( -1, 1 ); >;
	float g_flSpeed < UiGroup( "Adjustments,0/Speed,2/0" ); Default1( 2.5 ); Range1( 0, 5 ); >;
	float g_flMasterOpacity < UiGroup( "Adjustments,0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;

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
		float3 local1 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float4 local2 = float4( local1.xyz, 0 ).zzzw;
		float local3 = g_flTiling;
		float local4 = g_flSpeed;
		float local5 = g_flTime * local4;
		float2 local6 = TileAndOffsetUv( local2.xy, float2( local3, local3 ), float2( local5, local5 ) );
		float local7 = Simplex2D( local6 );
		float local8 = step( 0.005, local7 );
		float local9 = g_flMasterOpacity;
		float local10 = lerp( 0, local8, local9 );

		m.Emission = local0.xyz;
		m.Opacity = local10;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
