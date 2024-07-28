
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
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
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
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
};

VS
{
	#include "common/vertex.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( WPONoise, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tWPONoise < Channel( RGBA, Box( WPONoise ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	float g_flWPOStrength < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( -100, 100 ); >;
	float2 g_vWPOTiling < UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( -100,-100, 100,100 ); >;
	float2 g_vWPOScrollSpeed < UiGroup( ",0/,0/0" ); Default2( 0.1,0.1 ); Range2( -10,-10, 10,10 ); >;
	
	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		
		float l_0 = g_flWPOStrength;
		float2 l_1 = g_vWPOTiling;
		float2 l_2 = g_vWPOScrollSpeed;
		float2 l_3 = float2( g_flTime, g_flTime ) * l_2;
		float2 l_4 = TileAndOffsetUv( i.vTextureCoords.xy, l_1, l_3 );
		float4 l_5 = g_tWPONoise.SampleLevel( g_sSampler0, l_4, 0 );
		float l_6 = l_0 * l_5.r;
		i.vPositionWs.xyz += float3( l_6, l_6, l_6 );
		i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );
		
		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( WaterNoise, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( DistortNoise, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( MaskNoise, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( MacroNormal, Linear, 8, "NormalizeNormals", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tWaterNoise < Channel( RGBA, Box( WaterNoise ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tDistortNoise < Channel( RGBA, Box( DistortNoise ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tMaskNoise < Channel( RGBA, Box( MaskNoise ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tMacroNormal < Channel( RGBA, Box( MacroNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	float4 g_vWaterColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.03, 0.21, 0.50, 1.00 ); >;
	float4 g_vFoamColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.64, 0.83, 0.93, 1.00 ); >;
	float2 g_vDistortTiling < UiGroup( ",0/,0/0" ); Default2( 2,2 ); Range2( -100,-100, 100,100 ); >;
	float2 g_vDistortScrollSpeed < UiGroup( ",0/,0/0" ); Default2( 0.01,0.01 ); Range2( -10,-10, 10,10 ); >;
	float g_flDistortAmount < UiGroup( ",0/,0/0" ); Default1( 0.15 ); Range1( 0, 1 ); >;
	float2 g_vTiling < UiGroup( ",0/,0/0" ); Default2( 25,25 ); Range2( -100,-100, 100,100 ); >;
	float2 g_vScrollSpeed < UiGroup( ",0/,0/0" ); Default2( 0.1,0.1 ); Range2( -10,-10, 10,10 ); >;
	float g_flNoiseStrength < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0, 1 ); >;
	float2 g_vMaskTiling < UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( -100,-100, 100,100 ); >;
	float2 g_vMaskScrollSpeed < UiGroup( ",0/,0/0" ); Default2( -0.1,-0.1 ); Range2( -10,-10, 10,10 ); >;
	float2 g_vNormalTiling < UiGroup( ",0/,0/0" ); Default2( 2,2 ); Range2( -100,-100, 100,100 ); >;
	float2 g_vNormalScrollSpeed < UiGroup( ",0/,0/0" ); Default2( 0.01,0.01 ); Range2( -10,-10, 10,10 ); >;
	float g_flRoughness < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0, 1 ); >;
		
	float Overlay_blend( float a, float b )
	{
	    if ( a <= 0.5f )
	        return 2.0f * a * b;
	    else
	        return 1.0f - 2.0f * ( 1.0f - a ) * ( 1.0f - b );
	}
	
	float3 Overlay_blend( float3 a, float3 b )
	{
	    return float3(
	        Overlay_blend( a.r, b.r ),
	        Overlay_blend( a.g, b.g ),
	        Overlay_blend( a.b, b.b )
		);
	}
	
	float4 Overlay_blend( float4 a, float4 b, bool blendAlpha = false )
	{
	    return float4(
	        Overlay_blend( a.rgb, b.rgb ).rgb,
	        blendAlpha ? Overlay_blend( a.a, b.a ) : max( a.a, b.a )
	    );
	}
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float4 l_0 = g_vWaterColour;
		float4 l_1 = g_vFoamColour;
		float4 l_2 = saturate( lerp( l_0, Overlay_blend( l_0, l_1 ), 1 ) );
		float2 l_3 = g_vDistortTiling;
		float2 l_4 = g_vDistortScrollSpeed;
		float2 l_5 = float2( g_flTime, g_flTime ) * l_4;
		float2 l_6 = TileAndOffsetUv( i.vTextureCoords.xy, l_3, l_5 );
		float4 l_7 = Tex2DS( g_tDistortNoise, g_sSampler0, l_6 );
		float l_8 = g_flDistortAmount;
		float2 l_9 = lerp( l_6, float2( l_7.r, l_7.r ), l_8 );
		float2 l_10 = g_vTiling;
		float2 l_11 = g_vScrollSpeed;
		float2 l_12 = float2( g_flTime, g_flTime ) * l_11;
		float2 l_13 = TileAndOffsetUv( l_9, l_10, l_12 );
		float4 l_14 = Tex2DS( g_tWaterNoise, g_sSampler0, l_13 );
		float l_15 = g_flNoiseStrength;
		float l_16 = l_14.g * l_15;
		float4 l_17 = lerp( l_0, l_1, l_16 );
		float3 l_18 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), 10 );
		float3 l_19 = 1 - l_18;
		float2 l_20 = g_vMaskTiling;
		float2 l_21 = g_vMaskScrollSpeed;
		float2 l_22 = float2( g_flTime, g_flTime ) * l_21;
		float2 l_23 = TileAndOffsetUv( i.vTextureCoords.xy, l_20, l_22 );
		float4 l_24 = Tex2DS( g_tMaskNoise, g_sSampler0, l_23 );
		float3 l_25 = l_19 * float3( l_24.b, l_24.b, l_24.b );
		float4 l_26 = lerp( l_2, l_17, float4( l_25, 0 ) );
		float2 l_27 = g_vNormalTiling;
		float2 l_28 = g_vNormalScrollSpeed;
		float2 l_29 = float2( g_flTime, g_flTime ) * l_28;
		float2 l_30 = TileAndOffsetUv( i.vTextureCoords.xy, l_27, l_29 );
		float4 l_31 = Tex2DS( g_tMacroNormal, g_sSampler0, l_30 );
		float3 l_32 = DecodeNormal( l_31.xyz );
		float l_33 = g_flRoughness;
		
		m.Albedo = l_26.xyz;
		m.Opacity = 1;
		m.Normal = l_32;
		m.Roughness = l_33;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = i.vTextureCoords.xy;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
