
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
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
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
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

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	float4 g_vColour < UiType( Color ); UiGroup( "Colour,1/Emission,1/0" ); Default4( 0.00, 448.08, 500.00, 1.00 ); >;
	
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
		
		float4 l_0 = g_vColour;
		float4 l_1 = l_0 * float4( 1, 1, 1, 1 );
		float3 l_2 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float4 l_3 = float4( l_2, 0 ).zzzw;
		float l_4 = g_flTime * 2.5;
		float2 l_5 = TileAndOffsetUv( l_3.xy, float2( 1, 1 ), float2( l_4, l_4 ) );
		float l_6 = Simplex2D( l_5 );
		float l_7 = step( 0.005, l_6 );
		float l_8 = lerp( 0, l_7, 0.5 );
		
		m.Emission = l_1.xyz;
		m.Opacity = l_8;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		return ShadingModelStandard::Shade( i, m );
	}
}
