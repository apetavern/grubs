
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
	Forward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
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
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( NoiseOne, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_noise_01.png" ) );
	CreateInputTexture2D( NoiseTwo, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_noise_02.png" ) );
	Texture2D g_tNoiseOne < Channel( RGBA, Box( NoiseOne ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNoiseTwo < Channel( RGBA, Box( NoiseTwo ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float g_flNoiseOnePanSpeed < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float g_flNoiseTwoPanSpeed < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 5 ); >;
	
	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_1 = l_0.x;
		float l_2 = g_flNoiseOnePanSpeed;
		float l_3 = l_2 * g_flTime;
		float l_4 = l_0.y;
		float l_5 = l_3 + l_4;
		float4 l_6 = float4( l_1, l_5, 0, 0 );
		float4 l_7 = g_tNoiseOne.SampleLevel( g_sSampler0, l_6.xy, 0 );
		float2 l_8 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_9 = l_8.x;
		float l_10 = g_flNoiseTwoPanSpeed;
		float l_11 = l_10 * g_flTime;
		float l_12 = l_8.y;
		float l_13 = l_11 + l_12;
		float4 l_14 = float4( l_9, l_13, 0, 0 );
		float4 l_15 = g_tNoiseTwo.SampleLevel( g_sSampler0, l_14.xy, 0 );
		float4 l_16 = l_7 + l_15;
		float4 l_17 = float4( i.vNormalOs, 0 ) * l_16;
		i.vPositionWs.xyz += l_17.xyz;
		i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Gradient, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_gradient.png" ) );
	CreateInputTexture2D( NoiseOne, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_noise_01.png" ) );
	CreateInputTexture2D( NoiseTwo, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_noise_02.png" ) );
	CreateInputTexture2D( DistortionMask, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_bellmask.png" ) );
	CreateInputTexture2D( FlameMask, Linear, 8, "None", "_mask", ",0/,0/0", DefaultFile( "particles/fire/textures/fire_diamondmask.png" ) );
	Texture2D g_tGradient < Channel( RGBA, Box( Gradient ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNoiseOne < Channel( RGBA, Box( NoiseOne ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tNoiseTwo < Channel( RGBA, Box( NoiseTwo ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tDistortionMask < Channel( RGBA, Box( DistortionMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tFlameMask < Channel( RGBA, Box( FlameMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tFlameMask )
	TextureAttribute( RepresentativeTexture, g_tFlameMask )
	float g_flNoiseOnePanSpeed < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float g_flNoiseTwoPanSpeed < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 5 ); >;
	float g_flUVDistortionIntensity < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;
	float g_flFlameMaskDistortion < UiGroup( ",0/,0/0" ); Default1( 0.05 ); Range1( 0, 1 ); >;
	float4 g_vColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flEmissionStrength < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 100 ); >;
	float g_flSmoothStepIn < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flSmoothStepOut < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init( i );
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_1 = l_0.x;
		float l_2 = g_flNoiseOnePanSpeed;
		float l_3 = l_2 * g_flTime;
		float l_4 = l_0.y;
		float l_5 = l_3 + l_4;
		float4 l_6 = float4( l_1, l_5, 0, 0 );
		float4 l_7 = Tex2DS( g_tNoiseOne, g_sSampler1, l_6.xy );
		float2 l_8 = i.vTextureCoords.xy * float2( 1, 1 );
		float l_9 = l_8.x;
		float l_10 = g_flNoiseTwoPanSpeed;
		float l_11 = l_10 * g_flTime;
		float l_12 = l_8.y;
		float l_13 = l_11 + l_12;
		float4 l_14 = float4( l_9, l_13, 0, 0 );
		float4 l_15 = Tex2DS( g_tNoiseTwo, g_sSampler1, l_14.xy );
		float4 l_16 = l_7 + l_15;
		float4 l_17 = Tex2DS( g_tDistortionMask, g_sSampler1, i.vTextureCoords.xy );
		float4 l_18 = l_16 * l_17;
		float l_19 = g_flUVDistortionIntensity;
		float4 l_20 = l_18 * float4( l_19, l_19, l_19, l_19 );
		float2 l_21 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_22 = l_20 + float4( l_21, 0, 0 );
		float4 l_23 = Tex2DS( g_tGradient, g_sSampler0, l_22.xy );
		float l_24 = g_flFlameMaskDistortion;
		float4 l_25 = l_16 * float4( l_24, l_24, l_24, l_24 );
		float2 l_26 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 l_27 = l_25 + float4( l_26, 0, 0 );
		float4 l_28 = Tex2DS( g_tFlameMask, g_sSampler1, l_27.xy );
		float4 l_29 = l_23 * l_28;
		float4 l_30 = g_vColour;
		float4 l_31 = l_29 * l_30;
		float l_32 = g_flEmissionStrength;
		float4 l_33 = l_31 * float4( l_32, l_32, l_32, l_32 );
		float l_34 = g_flSmoothStepIn;
		float l_35 = g_flSmoothStepOut;
		float4 l_36 = smoothstep( l_34, l_35, l_29 );
		float4 l_37 = saturate( l_36 );
		
		m.Albedo = l_31.xyz;
		m.Emission = l_33.xyz;
		m.Opacity = l_37.x;
		m.Roughness = 1;
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
				
		return ShadingModelStandard::Shade( m );
	}
}
