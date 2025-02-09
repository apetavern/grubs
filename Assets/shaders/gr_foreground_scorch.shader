
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
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateInputTexture2D( Colour, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( BlendMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( ScorchColour, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( ScorchBlendMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( ScorchNormal, Linear, 8, "NormalizeNormals", "_normal", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Rough, Linear, 8, "None", "_rough", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( ScorchRough, Linear, 8, "None", "_rough", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( AO, Linear, 8, "None", "_ao", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( ScorchAO, Linear, 8, "None", "_ao", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColour < Channel( RGBA, Box( Colour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tBlendMask < Channel( RGBA, Box( BlendMask ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tScorchColour < Channel( RGBA, Box( ScorchColour ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tScorchLayer < Attribute( "ScorchLayer" ); >;
	Texture2D g_tScorchBlendMask < Channel( RGBA, Box( ScorchBlendMask ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tScorchNormal < Channel( RGBA, Box( ScorchNormal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tRough < Channel( RGBA, Box( Rough ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tScorchRough < Channel( RGBA, Box( ScorchRough ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tAO < Channel( RGBA, Box( AO ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tScorchAO < Channel( RGBA, Box( ScorchAO ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float g_flTiling < UiGroup( "Textures,0/,1/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float4 g_vTint_Colour < UiType( Color ); UiGroup( "Tint,2/,0/2" ); Default4( 0.41, 0.22, 0.11, 1.00 ); >;
	float g_flZPosition < UiGroup( "Position,0/Z,1/1" ); Default1( 0 ); Range1( 0, 2048 ); >;
	float g_flZSmoothing < UiGroup( "Position,0/Z,1/2" ); Default1( 512 ); Range1( 0, 2048 ); >;
	bool g_bTintDirectionToggle < UiGroup( "Tint,1/,0/1" ); Default( 0 ); >;
	bool g_bGradientTint < UiGroup( "Tint,0/,0/0" ); Default( 1 ); >;
	float4 g_vScorchTint_Colour < UiType( Color ); UiGroup( "Tint,2/,0/2" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float4 g_vScorchLayer_Params < Attribute( "ScorchLayer_Params" ); >;
	float g_flScorchBlendDistance < UiGroup( "Scorch,10/,0/10" ); Default1( 32 ); Range1( 0, 256 ); >;
		
	float4 TexTriplanar_Color( in Texture2D tTex, in SamplerState sSampler, float3 vPosition, float3 vNormal )
	{
		float2 uvX = vPosition.zy;
		float2 uvY = vPosition.xz;
		float2 uvZ = vPosition.xy;
	
		float3 triblend = saturate(pow(abs(vNormal), 4));
		triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);
	
		half3 axisSign = vNormal < 0 ? -1 : 1;
	
		uvX.x *= axisSign.x;
		uvY.x *= axisSign.y;
		uvZ.x *= -axisSign.z;
	
		float4 colX = Tex2DS( tTex, sSampler, uvX );
		float4 colY = Tex2DS( tTex, sSampler, uvY );
		float4 colZ = Tex2DS( tTex, sSampler, uvZ );
	
		return colX * triblend.x + colY * triblend.y + colZ * triblend.z;
	}
	
	float SoftLight_blend( float a, float b )
	{
	    if ( b <= 0.5f )
	        return 2.0f * a * b + a * a * ( 1.0f * 2.0f * b );
	    else 
	        return sqrt( a ) * ( 2.0f * b - 1.0f ) + 2.0f * a * (1.0f - b);
	}
	
	float3 SoftLight_blend( float3 a, float3 b )
	{
	    return float3(
	        SoftLight_blend( a.r, b.r ),
	        SoftLight_blend( a.g, b.g ),
	        SoftLight_blend( a.b, b.b )
		);
	}
	
	float4 SoftLight_blend( float4 a, float4 b, bool blendAlpha = false )
	{
	    return float4(
	        SoftLight_blend( a.rgb, b.rgb ).rgb,
	        blendAlpha ? SoftLight_blend( a.a, b.a ) : max( a.a, b.a )
	    );
	}
	
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
	
	float3 TexTriplanar_Normal( in Texture2D tTex, in SamplerState sSampler, float3 vPosition, float3 vNormal )
	{
		float2 uvX = vPosition.zy;
		float2 uvY = vPosition.xz;
		float2 uvZ = vPosition.xy;
	
		float3 triblend = saturate( pow( abs( vNormal ), 4 ) );
		triblend /= max( dot( triblend, half3( 1, 1, 1 ) ), 0.0001 );
	
		half3 axisSign = vNormal < 0 ? -1 : 1;
	
		uvX.x *= axisSign.x;
		uvY.x *= axisSign.y;
		uvZ.x *= -axisSign.z;
	
		float3 tnormalX = DecodeNormal( Tex2DS( tTex, sSampler, uvX ).xyz );
		float3 tnormalY = DecodeNormal( Tex2DS( tTex, sSampler, uvY ).xyz );
		float3 tnormalZ = DecodeNormal( Tex2DS( tTex, sSampler, uvZ ).xyz );
	
		tnormalX.x *= axisSign.x;
		tnormalY.x *= axisSign.y;
		tnormalZ.x *= -axisSign.z;
	
		tnormalX = half3( tnormalX.xy + vNormal.zy, vNormal.x );
		tnormalY = half3( tnormalY.xy + vNormal.xz, vNormal.y );
		tnormalZ = half3( tnormalZ.xy + vNormal.xy, vNormal.z );
	
		return normalize(
			tnormalX.zyx * triblend.x +
			tnormalY.xzy * triblend.y +
			tnormalZ.xyz * triblend.z +
			vNormal
		);
	}
	
	float3 Vec3OsToTs( float3 vVectorOs, float3 vNormalOs, float3 vTangentUOs, float3 vTangentVOs )
	{
		float3 vVectorTs;
		vVectorTs.x = dot( vVectorOs.xyz, vTangentUOs.xyz );
		vVectorTs.y = dot( vVectorOs.xyz, vTangentVOs.xyz );
		vVectorTs.z = dot( vVectorOs.xyz, vNormalOs.xyz );
		return vVectorTs.xyz;
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
		
		float3 l_0 = i.vPositionOs;
		float l_1 = g_flTiling;
		float l_2 = l_1 * 0.00390625;
		float3 l_3 = l_0 * float3( l_2, l_2, l_2 );
		float4 l_4 = TexTriplanar_Color( g_tColour, g_sSampler0, l_3, i.vNormalOs );
		float4 l_5 = g_vTint_Colour;
		float4 l_6 = TexTriplanar_Color( g_tBlendMask, g_sSampler0, l_3, i.vNormalOs );
		float4 l_7 = saturate( lerp( l_5, SoftLight_blend( l_5, l_4 ), l_6.r ) );
		float3 l_8 = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
		float l_9 = l_8.z;
		float l_10 = g_flZPosition;
		float l_11 = l_9 + l_10;
		float l_12 = g_flZSmoothing;
		float l_13 = l_11 / l_12;
		float l_14 = saturate( l_13 );
		float l_15 = 1 - l_14;
		float l_16 = g_bTintDirectionToggle ? l_14 : l_15;
		float4 l_17 = saturate( lerp( l_4, Overlay_blend( l_4, l_7 ), l_16 ) );
		float4 l_18 = saturate( lerp( l_5, l_4, 0.5 ) );
		float4 l_19 = g_bGradientTint ? l_17 : l_18;
		float4 l_20 = g_vScorchTint_Colour;
		float4 l_21 = TexTriplanar_Color( g_tScorchColour, g_sSampler0, l_3, i.vNormalOs );
		float4 l_22 = l_20 * l_21;
		float4 l_23 = g_vScorchLayer_Params;
		float4 l_24 = float4( l_23.r, l_23.r, 0, 0 );
		float3 l_25 = i.vPositionOs;
		float3 l_26 = float3( l_23.b, l_23.b, l_23.b ) * l_25;
		float4 l_27 = l_24 + float4( l_26, 0 );
		float4 l_28 = Tex2DS( g_tScorchLayer, g_sSampler1, l_27.xy );
		float l_29 = l_28.r - 0.5;
		float l_30 = l_29 * l_23.a;
		float l_31 = g_flScorchBlendDistance;
		float l_32 = l_23.a / l_31;
		float l_33 = l_30 * l_32;
		float l_34 = l_33 * -0.5;
		float4 l_35 = TexTriplanar_Color( g_tScorchBlendMask, g_sSampler0, l_3, i.vNormalOs );
		float l_36 = l_35.r - 0.5;
		float l_37 = l_36 * 32;
		float l_38 = l_34 - l_37;
		float l_39 = max( l_38, 0 );
		float l_40 = min( l_39, 1 );
		float4 l_41 = lerp( l_19, l_22, l_40 );
		float3 l_42 = TexTriplanar_Normal( g_tNormal, g_sSampler0, l_3, i.vNormalOs );
		float3 l_43 = TexTriplanar_Normal( g_tScorchNormal, g_sSampler0, l_3, i.vNormalOs );
		float3 l_44 = lerp( l_42, l_43, l_40 );
		float3 l_45 = Vec3OsToTs( l_44, i.vNormalOs.xyz, i.vTangentUOs_flTangentVSign.xyz, cross( i.vNormalOs.xyz, i.vTangentUOs_flTangentVSign.xyz ) * i.vTangentUOs_flTangentVSign.w );
		float4 l_46 = TexTriplanar_Color( g_tRough, g_sSampler0, l_3, i.vNormalOs );
		float4 l_47 = TexTriplanar_Color( g_tScorchRough, g_sSampler0, l_3, i.vNormalOs );
		float4 l_48 = lerp( l_46, float4( l_47.r, l_47.r, l_47.r, l_47.r ), l_40 );
		float4 l_49 = TexTriplanar_Color( g_tAO, g_sSampler0, l_3, i.vNormalOs );
		float4 l_50 = TexTriplanar_Color( g_tScorchAO, g_sSampler0, l_3, i.vNormalOs );
		float4 l_51 = lerp( l_49, float4( l_50.r, l_50.r, l_50.r, l_50.r ), l_40 );
		
		m.Albedo = l_41.xyz;
		m.Opacity = 1;
		m.Normal = l_45;
		m.Roughness = l_48.x;
		m.Metalness = 0;
		m.AmbientOcclusion = l_51.x;
		
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
