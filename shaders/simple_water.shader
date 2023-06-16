//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	Description = "Simple Water Shader for S&box";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
	{
		PixelInput o = ProcessVertex( i );
		
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"

	RenderState(BlendEnable, true);
    RenderState(SrcBlend, SRC_ALPHA);
    RenderState(DstBlend, INV_SRC_ALPHA);

	BoolAttribute(bWantsFBCopyTexture, true);
    BoolAttribute(translucent, true);

	//The speed of the distortion
	float g_flSpeed < Default(0.0); Range(0.0,1.0); >;

	//The base alpha of the material
    float g_baseAlph < Default(0.0); Range(0.0,1.0); >;

	//The scale of the distortion
    float g_flScale < Default(0.0); Range(0.0,100.0); >;
	float g_flDistortAmt < Default(0.0); Range(0.0,3.0); >;

	//The distortion of objects below the water
	float g_flOpDistort < Default(0.0); Range(0.0,3.0); >;
	float g_flOpacity < Default(0.0); Range(0.0,1.0); >;
	float g_flRoughness < Default(0.0); Range(0.0,1.0); >;
	
	//The normal map used for distortion
	CreateInputTexture2D( Distortion,   Srgb,       8,  "",     "_color",  "Material,10/90", Default3( 1.0, 1.0, 1.0 ) );
    CreateTexture2D( g_tDistortion ) < Channel( RGBA, Box(Distortion ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

	//Frame buffer copy texture
	CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute("FrameBufferCopyTexture");   SrgbRead( false ); Filter(MIN_MAG_MIP_LINEAR);    AddressU( MIRROR );     AddressV( MIRROR ); > ;    
	//
	// Main
	//
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		//Get the uv coordinates
		float2 uv = i.vTextureCoords.xy;

		//Layer two levels of distortion over each other and normalize the result
		float3 distort_layer_1 = Tex2D( g_tDistortion, (uv * g_flScale) + float2((g_flTime * -g_flSpeed) + 0.5,0.5) ).rgb;
		float3 distort_layer_2 = Tex2D( g_tDistortion, (uv * g_flScale) + float2(g_flTime * g_flSpeed,g_flTime * g_flSpeed) ).rgb;
		float3 distortFinal = (distort_layer_1 + distort_layer_2) / 2.0;
		
		//Get the position of the pixel in screen space
		uint nView = uint(0);
		float4 vPositionPs = Position4WsToPsMultiview(nView, float4(i.vPositionWithOffsetWs.xyz, 0));
		float2 vPositionSs = vPositionPs.xy / vPositionPs.w;
        vPositionSs = vPositionSs * 0.5 + 0.5;
    	vPositionSs.y = 1.0 - vPositionSs.y;

		//Get the frame buffer copy texture and the material
		//TODO: This is a little dubious, objects in front will sort of have some shimmering around their borders. 
		//I can probably mess with the depth texture or something to get around this, but I'm not sure how to do that yet.
		float3 frame = Tex2D( g_tFrameBufferCopyTexture, vPositionSs + distortFinal * g_flOpDistort ).rgb;
		Material m = Material::From( i );
		
		//Apply the distortion to the normal
		m.Normal = TransformNormal( i, DecodeNormal(lerp(float3(0.5,0.5,1),distortFinal,g_flDistortAmt))    ) ;
		
		//Apply the opacity and roughness
		
		m.Albedo = lerp(m.Albedo,frame.rgb,g_flOpacity);

		m.Metalness = 0.0;
		m.Roughness = g_flRoughness;

		return ShadingModelStandard::Shade(i, m);
	}
}
