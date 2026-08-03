Shader "NatureManufacture Shaders/HD SRP Cross Road Material"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_TextureSample1( "Second Road Noise Mask", 2D ) = "white" {}
		_SecondRoadNoiseMaskPower( "Second Road Noise Mask Power", Range( 0, 10 ) ) = 0.1
		_SecondRoadNoiseMaskTreshold( "Second Road Noise Mask Treshold", Range( 0, 10 ) ) = 1
		_MainRoadColor( "Main Road Color", Color ) = ( 1, 1, 1, 1 )
		_MainRoadBrightness( "Main Road Brightness", Float ) = 1
		_MainTex( "Main Road Albedo_T", 2D ) = "white" {}
		[Toggle] _MainRoadUV3( "Main Road UV3", Float ) = 0
		_MainRoadAlphaCutOut( "Main Road Alpha CutOut", Range( 0, 2 ) ) = 1
		_BumpMap( "Main Road Normal", 2D ) = "bump" {}
		_BumpScale( "Main Road BumpScale", Range( 0, 5 ) ) = 0
		_MetalicRAmbientOcclusionGHeightBEmissionA( "Main Road Metallic (R) Ambient Occlusion (G) Height (B) Smoothness (A)", 2D ) = "white" {}
		_MainRoadMetalicPower( "Main Road Metalic Power", Range( 0, 2 ) ) = 0
		_MainRoadAmbientOcclusionPower( "Main Road Ambient Occlusion Power", Range( 0, 1 ) ) = 1
		_MainRoadSmoothnessPower( "Main Road Smoothness Power", Range( 0, 2 ) ) = 1
		_SecondRoadColor( "Second Road Color", Color ) = ( 1, 1, 1, 1 )
		_MainRoadParallaxPower( "Main Road Parallax Power", Range( 0, 0.1 ) ) = 0
		_SecondRoadBrightness( "Second Road Brightness", Float ) = 1
		_TextureSample3( "Second Road Albedo_T", 2D ) = "white" {}
		[Toggle] _SecondRoadUV3( "Second Road UV3", Float ) = 0
		[Toggle( _IGNORESECONDROADALPHA_ON )] _IgnoreSecondRoadAlpha( "Ignore Second Road Alpha", Float ) = 0
		_SecondRoadAlphaCutOut( "Second Road Alpha CutOut", Range( 0, 2 ) ) = 1
		_SecondRoadNormal( "Second Road Normal", 2D ) = "bump" {}
		_SecondRoadNormalScale( "Second Road Normal Scale", Range( 0, 5 ) ) = 0
		_SecondRoadNormalBlend( "Second Road Normal Blend", Range( 0, 1 ) ) = 0.8
		_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA( "Second Road Metallic (R) Ambient occlusion (G) Height (B) Smoothness (A)", 2D ) = "white" {}
		_SecondRoadMetalicPower( "Second Road Metalic Power", Range( 0, 2 ) ) = 1
		_SecondRoadAmbientOcclusionPower( "Second Road Ambient Occlusion Power", Range( 0, 1 ) ) = 1
		_SecondRoadSmoothnessPower( "Second Road Smoothness Power", Range( 0, 2 ) ) = 1
		_CrossRoadColor( "Cross Road Color", Color ) = ( 1, 1, 1, 1 )
		_CrossRoadBrightness( "Cross Road Brightness", Float ) = 1
		_SecondRoadParallaxPower( "Second Road Parallax Power", Range( -0.1, 0.1 ) ) = 0
		_TextureSample4( "Cross Road Albedo_T", 2D ) = "white" {}
		[Toggle] _CrossRoadUV3( "Cross Road UV3", Float ) = 0
		[Toggle( _IGNORECROSSROADALPHA_ON )] _IgnoreCrossRoadAlpha( "Ignore Cross Road Alpha", Float ) = 0
		_CrossRoadAlphaCutOut( "Cross Road Alpha CutOut", Range( 0, 2 ) ) = 1
		_CrossRoadNormal( "Cross Road Normal", 2D ) = "bump" {}
		_CrossRoadNormalScale( "Cross Road Normal Scale", Range( 0, 5 ) ) = 0
		_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA( "Cross Road Metallic (R) Ambient occlusion (G) Height (B) Smoothness (A)", 2D ) = "white" {}
		_CrossRoadMetallicPower( "Cross Road Metallic Power", Range( 0, 2 ) ) = 1.590408
		_CrossRoadAmbientOcclusionPower( "Cross Road Ambient Occlusion Power", Range( 0, 1 ) ) = 1
		_CrossRoadSmoothnessPower( "Cross Road Smoothness Power", Range( 0, 2 ) ) = 1
		_DetailMask( "DetailMask (A)", 2D ) = "white" {}
		_CrossRoadParallaxPower( "Cross Road Parallax Power", Range( -0.1, 0.1 ) ) = 0
		_Float3( "Cross Road Detail Albedo Power", Range( 0, 2 ) ) = 2
		_Float2( "Second Road Detail Albedo Power", Range( 0, 2 ) ) = 0
		[Header(Terrain Z Fighting Offset)]
		_OffsetFactor("Offset Factor", Range(-10.0, 0.0)) = 0
		_OffsetUnit("Offset Unit", Range(-10.0, 0.0)) = 0

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		//_InstancedTerrainNormals("Instanced Terrain Normals", Float) = 1.0

		[ToggleOff(_SPECULARHIGHLIGHTS_OFF)] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		//[HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		//[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1

		//[HideInInspector] _XRMotionVectorsPass("_XRMotionVectorsPass", Float) = 1

		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
	}

	SubShader
	{
		LOD 0

		

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Lit" }
		Offset[_OffsetFactor],[_OffsetUnit]

		Cull Back
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#if ( SHADER_TARGET > 35 ) && defined( SHADER_API_GLES3 )
			#error For WebGL2/GLES3, please set your shader target to 3.5 via SubShader options. URP shaders in ASE use target 4.5 by default.
		#endif

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			Offset[_OffsetFactor],[_OffsetUnit]

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
			#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ USE_LEGACY_LIGHTMAPS

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_FORWARD

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 lightmapUVOrVertexSH : TEXCOORD3;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				#if defined(USE_APV_PROBE_OCCLUSION)
					float4 probeOcclusion : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _DetailMask;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_texcoord7.xy = input.texcoord.xy;
				output.ase_texcoord7.zw = input.texcoord2.xy;
				output.ase_color = input.ase_color;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif
				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				OUTPUT_SH4(vertexInput.positionWS, normalInput.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion);

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						output.fogFactorAndVertexLight.x = ComputeFogFactor(vertexInput.positionCS.z);
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = input.texcoord1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = input.texcoord2;
				#endif
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#endif
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#if defined( ASE_DEPTH_WRITE_ON )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out uint outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined( _SURFACE_TYPE_TRANSPARENT )
					const bool isTransparent = true;
				#else
					const bool isTransparent = false;
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWS );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float2 uv_MainTex = input.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord7.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord7.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float2 uv_TextureSample3 = input.ase_texcoord7.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord7.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord7.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				float4 temp_output_540_0 = ( ( _SecondRoadBrightness * tex2DNode537 ) * _SecondRoadColor );
				float2 uv_DetailMask = input.ase_texcoord7.xy * _DetailMask_ST.xy + _DetailMask_ST.zw;
				float4 tex2DNode481 = tex2D( _DetailMask, uv_DetailMask );
				float4 lerpResult618 = lerp( temp_output_540_0 , temp_output_540_0 , ( _Float2 * tex2DNode481.a ));
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord7.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float4 lerpResult592 = lerp( ( ( _MainRoadBrightness * tex2DNode1 ) * _MainRoadColor ) , lerpResult618 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord7.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord7.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord7.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				float4 temp_output_654_0 = ( ( _CrossRoadBrightness * tex2DNode638 ) * _CrossRoadColor );
				float4 lerpResult653 = lerp( temp_output_654_0 , temp_output_654_0 , ( tex2DNode481.a * _Float3 ));
				float4 lerpResult852 = lerp( lerpResult653 , lerpResult618 , break496.x);
				float4 lerpResult644 = lerp( lerpResult592 , lerpResult852 , break496.y);
				
				float3 unpack4 = UnpackNormalScale( tex2D( _BumpMap, Offset996 ), _BumpScale );
				unpack4.z = lerp( 1, unpack4.z, saturate(_BumpScale) );
				float3 tex2DNode4 = unpack4;
				float3 lerpResult479 = lerp( tex2DNode4 , tex2DNode4 , tex2DNode481.a);
				float3 unpack535 = UnpackNormalScale( tex2D( _SecondRoadNormal, Offset957 ), _SecondRoadNormalScale );
				unpack535.z = lerp( 1, unpack535.z, saturate(_SecondRoadNormalScale) );
				float3 tex2DNode535 = unpack535;
				float3 lerpResult570 = lerp( lerpResult479 , tex2DNode535 , _SecondRoadNormalBlend);
				float3 lerpResult617 = lerp( tex2DNode535 , lerpResult570 , tex2DNode481.a);
				float3 lerpResult593 = lerp( lerpResult479 , lerpResult617 , break496.x);
				float3 unpack637 = UnpackNormalScale( tex2D( _CrossRoadNormal, Offset956 ), _CrossRoadNormalScale );
				unpack637.z = lerp( 1, unpack637.z, saturate(_CrossRoadNormalScale) );
				float3 lerpResult848 = lerp( unpack637 , lerpResult617 , break496.x);
				float3 lerpResult640 = lerp( lerpResult593 , lerpResult848 , break496.y);
				
				float4 tex2DNode2 = tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset996 );
				float4 tex2DNode536 = tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset957 );
				float temp_output_547_0 = ( tex2DNode536.r * _SecondRoadMetalicPower );
				float lerpResult601 = lerp( ( tex2DNode2.r * _MainRoadMetalicPower ) , temp_output_547_0 , break496.x);
				float4 tex2DNode639 = tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset956 );
				float lerpResult850 = lerp( ( _CrossRoadMetallicPower * tex2DNode639.r ) , temp_output_547_0 , 0.0);
				float lerpResult643 = lerp( lerpResult601 , lerpResult850 , break496.y);
				
				float temp_output_548_0 = ( _SecondRoadSmoothnessPower * tex2DNode536.a );
				float lerpResult594 = lerp( ( tex2DNode2.a * _MainRoadSmoothnessPower ) , temp_output_548_0 , break496.x);
				float lerpResult847 = lerp( ( tex2DNode639.a * _CrossRoadSmoothnessPower ) , temp_output_548_0 , break496.x);
				float lerpResult645 = lerp( lerpResult594 , lerpResult847 , break496.y);
				
				float clampResult96 = clamp( tex2DNode2.g , ( 1.0 - _MainRoadAmbientOcclusionPower ) , 1.0 );
				float clampResult546 = clamp( tex2DNode536.g , ( 1.0 - _SecondRoadAmbientOcclusionPower ) , 1.0 );
				float lerpResult602 = lerp( clampResult96 , clampResult546 , break496.x);
				float clampResult662 = clamp( tex2DNode639.g , ( 1.0 - _CrossRoadAmbientOcclusionPower ) , 1.0 );
				float lerpResult851 = lerp( clampResult662 , clampResult546 , break496.x);
				float lerpResult642 = lerp( lerpResult602 , lerpResult851 , break496.y);
				
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float3 BaseColor = lerpResult644.rgb;
				float3 Normal = lerpResult640;
				float3 Specular = 0.5;
				float Metallic = lerpResult643;
				float Smoothness = lerpResult645;
				float Occlusion = lerpResult642;
				float3 Emission = 0;
				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
					float AlphaClipThresholdShadow = 0.5;
				#endif
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _CLEARCOAT
					float CoatMask = 0;
					float CoatSmoothness = 0;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.viewDirectionWS = ViewDirWS;
				inputData.shadowCoord = ShadowCoord;

				#ifdef _NORMALMAP
						#if _NORMAL_DROPOFF_TS
							inputData.normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							inputData.normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							inputData.normalWS = Normal;
						#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = NormalWS;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = input.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(_SCREEN_SPACE_IRRADIANCE)
					inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
				#elif defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
					inputData.bakedGI = SAMPLE_GI( SH, GetAbsolutePositionWS(inputData.positionWS),
						inputData.normalWS,
						inputData.viewDirectionWS,
						input.positionCS.xy,
						input.probeOcclusion,
						inputData.shadowMask );
				#else
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
					#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
					#if defined(USE_APV_PROBE_OCCLUSION)
						inputData.probeOcclusion = input.probeOcclusion;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = BaseColor;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;

				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
				#endif

				#ifdef ASE_LIGHTING_SIMPLE
					half4 color = UniversalFragmentBlinnPhong( inputData, surfaceData);
				#else
					half4 color = UniversalFragmentPBR( inputData, surfaceData);
				#endif

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;

					#define SUM_LIGHT_TRANSMISSION(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 transmission = max( 0, -dot( inputData.normalWS, Light.direction ) ) * atten * Transmission;\
						color.rgb += BaseColor * transmission;

					SUM_LIGHT_TRANSMISSION( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_CLUSTER_LIGHT_LOOP
							[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSMISSION( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSMISSION( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					#define SUM_LIGHT_TRANSLUCENCY(Light)\
						float3 atten = Light.color * Light.distanceAttenuation;\
						atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
						half3 lightDir = Light.direction + inputData.normalWS * normal;\
						half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );\
						half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;\
						color.rgb += BaseColor * translucency * strength;

					SUM_LIGHT_TRANSLUCENCY( GetMainLight( inputData.shadowCoord ) );

					#if defined(_ADDITIONAL_LIGHTS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						uint pixelLightCount = GetAdditionalLightsCount();
						#if USE_CLUSTER_LIGHT_LOOP
							[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

								Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
								#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
								#endif
								{
									SUM_LIGHT_TRANSLUCENCY( light );
								}
							}
						#endif
						LIGHT_LOOP_BEGIN( pixelLightCount )
							Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
							#ifdef _LIGHT_LAYERS
							if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
							#endif
							{
								SUM_LIGHT_TRANSLUCENCY( light );
							}
						LIGHT_LOOP_END
					#endif
				}
				#endif

				#ifdef ASE_REFRACTION
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( NormalWS,0 ) ).xyz * ( 1.0 - dot( NormalWS, ViewDirWS ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						color.rgb = MixFog(color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					outRenderingLayers = EncodeMeshRenderingLayer();
				#endif

				#if defined( ASE_OPAQUE_KEEP_ALPHA )
					return half4( color.rgb, color.a );
				#else
					return half4( color.rgb, OutputAlpha( color.a, isTransparent ) );
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }
			Offset[_OffsetFactor],[_OffsetUnit]

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			float3 _LightDirection;
			float3 _LightPosition;

			
			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord2.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				output.ase_texcoord1.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				float3 positionWS = TransformObjectToWorld( input.positionOS.xyz );
				float3 normalWS = TransformObjectToWorldDir(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				//code for UNITY_REVERSED_Z is moved into Shadows.hlsl from 6000.0.22 and or higher
				positionCS = ApplyShadowClamping(positionCS);

				output.positionCS = positionCS;
				output.positionWS = positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord2 = input.ase_texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(	PackedVaryings input
						#if defined( ASE_DEPTH_WRITE_ON )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float2 uv_MainTex = input.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord1.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord1.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord2.xyz;
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				float2 uv_TextureSample3 = input.ase_texcoord1.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord1.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord1.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord1.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord1.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
					float AlphaClipThresholdShadow = 0.5;
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					#if defined( _ALPHATEST_SHADOW_ON )
						AlphaDiscard( Alpha, AlphaClipThresholdShadow );
					#else
						AlphaDiscard( Alpha, AlphaClipThreshold );
					#endif
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }
			Offset[_OffsetFactor],[_OffsetUnit]

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord2.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				output.ase_texcoord1.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord2 = input.ase_texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(	PackedVaryings input
						#if defined( ASE_DEPTH_WRITE_ON )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );

				float2 uv_MainTex = input.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord1.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord1.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord2.xyz;
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				float2 uv_TextureSample3 = input.ase_texcoord1.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord1.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord1.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord1.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord1.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float Alpha = lerpResult641;
				float AlphaClipThreshold = 0.5;

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }
			Offset[_OffsetFactor],[_OffsetUnit]

			Cull Off

			HLSLPROGRAM
			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300

			#pragma shader_feature EDITOR_VISUALIZATION

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				#ifdef EDITOR_VISUALIZATION
					float4 VizUV : TEXCOORD1;
					float4 LightCoord : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _DetailMask;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord4.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord5.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord6.xyz = ase_bitangentWS;
				
				output.ase_texcoord3.xy = input.texcoord.xy;
				output.ase_texcoord3.zw = input.texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord4.w = 0;
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				#ifdef EDITOR_VISUALIZATION
					float2 VizUV = 0;
					float4 LightCoord = 0;
					UnityEditorVizData(input.positionOS.xyz, input.texcoord.xy, input.texcoord1.xy, input.texcoord2.xy, VizUV, LightCoord);
					output.VizUV = float4(VizUV, 0, 0);
					output.LightCoord = LightCoord;
				#endif

				output.positionCS = MetaVertexPosition( input.positionOS, input.texcoord1.xy, input.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				output.positionWS = TransformObjectToWorld( input.positionOS.xyz );
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				output.texcoord1 = input.texcoord1;
				output.texcoord2 = input.texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;

				float2 uv_MainTex = input.ase_texcoord3.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord3.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord3.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord4.xyz;
				float3 ase_normalWS = input.ase_texcoord5.xyz;
				float3 ase_bitangentWS = input.ase_texcoord6.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float2 uv_TextureSample3 = input.ase_texcoord3.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord3.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord3.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				float4 temp_output_540_0 = ( ( _SecondRoadBrightness * tex2DNode537 ) * _SecondRoadColor );
				float2 uv_DetailMask = input.ase_texcoord3.xy * _DetailMask_ST.xy + _DetailMask_ST.zw;
				float4 tex2DNode481 = tex2D( _DetailMask, uv_DetailMask );
				float4 lerpResult618 = lerp( temp_output_540_0 , temp_output_540_0 , ( _Float2 * tex2DNode481.a ));
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord3.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float4 lerpResult592 = lerp( ( ( _MainRoadBrightness * tex2DNode1 ) * _MainRoadColor ) , lerpResult618 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord3.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord3.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord3.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				float4 temp_output_654_0 = ( ( _CrossRoadBrightness * tex2DNode638 ) * _CrossRoadColor );
				float4 lerpResult653 = lerp( temp_output_654_0 , temp_output_654_0 , ( tex2DNode481.a * _Float3 ));
				float4 lerpResult852 = lerp( lerpResult653 , lerpResult618 , break496.x);
				float4 lerpResult644 = lerp( lerpResult592 , lerpResult852 , break496.y);
				
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float3 BaseColor = lerpResult644.rgb;
				float3 Emission = 0;
				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BaseColor;
				metaInput.Emission = Emission;
				#ifdef EDITOR_VISUALIZATION
					metaInput.VizUV = input.VizUV.xy;
					metaInput.LightCoord = input.LightCoord;
				#endif

				return UnityMetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }
			Offset[_OffsetFactor],[_OffsetUnit]

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _DetailMask;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_TRANSFER_INSTANCE_ID( input, output );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord2.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				output.ase_texcoord1.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord2 = input.ase_texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;

				float2 uv_MainTex = input.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord1.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord1.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord2.xyz;
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float2 uv_TextureSample3 = input.ase_texcoord1.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord1.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				float4 temp_output_540_0 = ( ( _SecondRoadBrightness * tex2DNode537 ) * _SecondRoadColor );
				float2 uv_DetailMask = input.ase_texcoord1.xy * _DetailMask_ST.xy + _DetailMask_ST.zw;
				float4 tex2DNode481 = tex2D( _DetailMask, uv_DetailMask );
				float4 lerpResult618 = lerp( temp_output_540_0 , temp_output_540_0 , ( _Float2 * tex2DNode481.a ));
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord1.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float4 lerpResult592 = lerp( ( ( _MainRoadBrightness * tex2DNode1 ) * _MainRoadColor ) , lerpResult618 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord1.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord1.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				float4 temp_output_654_0 = ( ( _CrossRoadBrightness * tex2DNode638 ) * _CrossRoadColor );
				float4 lerpResult653 = lerp( temp_output_654_0 , temp_output_654_0 , ( tex2DNode481.a * _Float3 ));
				float4 lerpResult852 = lerp( lerpResult653 , lerpResult618 , break496.x);
				float4 lerpResult644 = lerp( lerpResult592 , lerpResult852 , break496.y);
				
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float3 BaseColor = lerpResult644.rgb;
				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
				#endif

				half4 color = half4(BaseColor, Alpha );

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }
			Offset[_OffsetFactor],[_OffsetUnit]

			ZWrite On
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
			//#define SHADERPASS SHADERPASS_DEPTHNORMALS

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				half4 texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _BumpMap;
			sampler2D _MainTex;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _DetailMask;
			sampler2D _SecondRoadNormal;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _TextureSample1;
			sampler2D _CrossRoadNormal;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_texcoord3.xy = input.texcoord.xy;
				output.ase_texcoord3.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				output.texcoord = input.texcoord;
				output.ase_texcoord2 = input.ase_texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				output.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			void frag(	PackedVaryings input
						, out half4 outNormalWS : SV_Target0
						#if defined( ASE_DEPTH_WRITE_ON )
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out uint outRenderingLayers : SV_Target1
						#endif
						 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( input.positionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float2 uv_MainTex = input.ase_texcoord3.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord3.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord3.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float3 unpack4 = UnpackNormalScale( tex2D( _BumpMap, Offset996 ), _BumpScale );
				unpack4.z = lerp( 1, unpack4.z, saturate(_BumpScale) );
				float3 tex2DNode4 = unpack4;
				float2 uv_DetailMask = input.ase_texcoord3.xy * _DetailMask_ST.xy + _DetailMask_ST.zw;
				float4 tex2DNode481 = tex2D( _DetailMask, uv_DetailMask );
				float3 lerpResult479 = lerp( tex2DNode4 , tex2DNode4 , tex2DNode481.a);
				float2 uv_TextureSample3 = input.ase_texcoord3.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord3.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord3.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float3 unpack535 = UnpackNormalScale( tex2D( _SecondRoadNormal, Offset957 ), _SecondRoadNormalScale );
				unpack535.z = lerp( 1, unpack535.z, saturate(_SecondRoadNormalScale) );
				float3 tex2DNode535 = unpack535;
				float3 lerpResult570 = lerp( lerpResult479 , tex2DNode535 , _SecondRoadNormalBlend);
				float3 lerpResult617 = lerp( tex2DNode535 , lerpResult570 , tex2DNode481.a);
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord3.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float3 lerpResult593 = lerp( lerpResult479 , lerpResult617 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord3.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord3.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord3.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float3 unpack637 = UnpackNormalScale( tex2D( _CrossRoadNormal, Offset956 ), _CrossRoadNormalScale );
				unpack637.z = lerp( 1, unpack637.z, saturate(_CrossRoadNormalScale) );
				float3 lerpResult848 = lerp( unpack637 , lerpResult617 , break496.x);
				float3 lerpResult640 = lerp( lerpResult593 , lerpResult848 , break496.y);
				
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float3 Normal = lerpResult640;
				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(NormalWS);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = NormalWS;
					#endif
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					outRenderingLayers = EncodeMeshRenderingLayer();
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }
			Offset[_OffsetFactor],[_OffsetUnit]

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			// Deferred Rendering Path does not support the OpenGL-based graphics API:
			// Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
			#pragma exclude_renderers glcore gles3 

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ USE_LEGACY_LIGHTMAPS
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
			#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SHADERPASS SHADERPASS_GBUFFER

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#if defined( UNITY_INSTANCING_ENABLED ) && defined( ASE_INSTANCED_TERRAIN ) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
				float4 lightmapUVOrVertexSH : TEXCOORD3;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				#if defined(USE_APV_PROBE_OCCLUSION)
					float4 probeOcclusion : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _DetailMask;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"

			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.ase_texcoord7.xy = input.texcoord.xy;
				output.ase_texcoord7.zw = input.texcoord2.xy;
				output.ase_color = input.ase_color;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;
				input.tangentOS = input.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

				OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				OUTPUT_SH4(vertexInput.positionWS, normalInput.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.lightmapUVOrVertexSH.xyz, output.probeOcclusion);

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						// @diogo: no fog applied in GBuffer
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					output.tangentWS.zw = input.texcoord.xy;
					output.tangentWS.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					float4 texcoord1 : TEXCOORD1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					float4 texcoord2 : TEXCOORD2;
				#endif
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.texcoord = input.texcoord;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = input.texcoord1;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = input.texcoord2;
				#endif
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				#if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
					output.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
					output.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				#endif
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			GBufferFragOutput frag ( PackedVaryings input
								#if defined( ASE_DEPTH_WRITE_ON )
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord( input.positionWS );
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif

				// @diogo: mikktspace compliant
				float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
				float4 ShadowCoord = shadowCoord;
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
				float4 ScreenPos = ComputeScreenPos( ClipPos );
				float3 TangentWS = input.tangentWS.xyz * renormFactor;
				float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
				float3 NormalWS = input.normalWS * renormFactor;

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float2 sampleCoords = (input.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
					BitangentWS = cross(NormalWS, -TangentWS);
				#endif

				float2 uv_MainTex = input.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord7.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord7.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
				float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
				float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float2 uv_TextureSample3 = input.ase_texcoord7.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord7.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord7.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				float4 temp_output_540_0 = ( ( _SecondRoadBrightness * tex2DNode537 ) * _SecondRoadColor );
				float2 uv_DetailMask = input.ase_texcoord7.xy * _DetailMask_ST.xy + _DetailMask_ST.zw;
				float4 tex2DNode481 = tex2D( _DetailMask, uv_DetailMask );
				float4 lerpResult618 = lerp( temp_output_540_0 , temp_output_540_0 , ( _Float2 * tex2DNode481.a ));
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord7.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float4 lerpResult592 = lerp( ( ( _MainRoadBrightness * tex2DNode1 ) * _MainRoadColor ) , lerpResult618 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord7.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord7.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord7.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				float4 temp_output_654_0 = ( ( _CrossRoadBrightness * tex2DNode638 ) * _CrossRoadColor );
				float4 lerpResult653 = lerp( temp_output_654_0 , temp_output_654_0 , ( tex2DNode481.a * _Float3 ));
				float4 lerpResult852 = lerp( lerpResult653 , lerpResult618 , break496.x);
				float4 lerpResult644 = lerp( lerpResult592 , lerpResult852 , break496.y);
				
				float3 unpack4 = UnpackNormalScale( tex2D( _BumpMap, Offset996 ), _BumpScale );
				unpack4.z = lerp( 1, unpack4.z, saturate(_BumpScale) );
				float3 tex2DNode4 = unpack4;
				float3 lerpResult479 = lerp( tex2DNode4 , tex2DNode4 , tex2DNode481.a);
				float3 unpack535 = UnpackNormalScale( tex2D( _SecondRoadNormal, Offset957 ), _SecondRoadNormalScale );
				unpack535.z = lerp( 1, unpack535.z, saturate(_SecondRoadNormalScale) );
				float3 tex2DNode535 = unpack535;
				float3 lerpResult570 = lerp( lerpResult479 , tex2DNode535 , _SecondRoadNormalBlend);
				float3 lerpResult617 = lerp( tex2DNode535 , lerpResult570 , tex2DNode481.a);
				float3 lerpResult593 = lerp( lerpResult479 , lerpResult617 , break496.x);
				float3 unpack637 = UnpackNormalScale( tex2D( _CrossRoadNormal, Offset956 ), _CrossRoadNormalScale );
				unpack637.z = lerp( 1, unpack637.z, saturate(_CrossRoadNormalScale) );
				float3 lerpResult848 = lerp( unpack637 , lerpResult617 , break496.x);
				float3 lerpResult640 = lerp( lerpResult593 , lerpResult848 , break496.y);
				
				float4 tex2DNode2 = tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset996 );
				float4 tex2DNode536 = tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset957 );
				float temp_output_547_0 = ( tex2DNode536.r * _SecondRoadMetalicPower );
				float lerpResult601 = lerp( ( tex2DNode2.r * _MainRoadMetalicPower ) , temp_output_547_0 , break496.x);
				float4 tex2DNode639 = tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset956 );
				float lerpResult850 = lerp( ( _CrossRoadMetallicPower * tex2DNode639.r ) , temp_output_547_0 , 0.0);
				float lerpResult643 = lerp( lerpResult601 , lerpResult850 , break496.y);
				
				float temp_output_548_0 = ( _SecondRoadSmoothnessPower * tex2DNode536.a );
				float lerpResult594 = lerp( ( tex2DNode2.a * _MainRoadSmoothnessPower ) , temp_output_548_0 , break496.x);
				float lerpResult847 = lerp( ( tex2DNode639.a * _CrossRoadSmoothnessPower ) , temp_output_548_0 , break496.x);
				float lerpResult645 = lerp( lerpResult594 , lerpResult847 , break496.y);
				
				float clampResult96 = clamp( tex2DNode2.g , ( 1.0 - _MainRoadAmbientOcclusionPower ) , 1.0 );
				float clampResult546 = clamp( tex2DNode536.g , ( 1.0 - _SecondRoadAmbientOcclusionPower ) , 1.0 );
				float lerpResult602 = lerp( clampResult96 , clampResult546 , break496.x);
				float clampResult662 = clamp( tex2DNode639.g , ( 1.0 - _CrossRoadAmbientOcclusionPower ) , 1.0 );
				float lerpResult851 = lerp( clampResult662 , clampResult546 , break496.x);
				float lerpResult642 = lerp( lerpResult602 , lerpResult851 , break496.y);
				
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float3 BaseColor = lerpResult644.rgb;
				float3 Normal = lerpResult640;
				float3 Specular = 0.5;
				float Metallic = lerpResult643;
				float Smoothness = lerpResult645;
				float Occlusion = lerpResult642;
				float3 Emission = 0;
				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
					float AlphaClipThresholdShadow = 0.5;
				#endif
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#if defined( _ALPHATEST_ON )
					AlphaDiscard( Alpha, AlphaClipThreshold );
				#endif

				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && defined(ASE_CHANGES_WORLD_POS)
					ShadowCoord = TransformWorldToShadowCoord( PositionWS );
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = PositionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
				inputData.shadowCoord = ShadowCoord;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
						inputData.normalWS = TransformTangentToWorld(Normal, half3x3( TangentWS, BitangentWS, NormalWS ));
					#elif _NORMAL_DROPOFF_OS
						inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
						inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = NormalWS;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( ViewDirWS );

				#ifdef ASE_FOG
					// @diogo: no fog applied in GBuffer
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				#if defined( ENABLE_TERRAIN_PERPIXEL_NORMAL )
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = input.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(_SCREEN_SPACE_IRRADIANCE)
					inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
				#elif defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
					inputData.bakedGI = SAMPLE_GI(SH,
						GetAbsolutePositionWS(inputData.positionWS),
						inputData.normalWS,
						inputData.viewDirectionWS,
						input.positionCS.xy,
						input.probeOcclusion,
						inputData.shadowMask);
				#else
					inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
					inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
					#if defined(USE_APV_PROBE_OCCLUSION)
						inputData.probeOcclusion = input.probeOcclusion;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(input.positionCS,
						BaseColor,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData(BaseColor, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);

				color.rgb = GlobalIllumination(brdfData, (BRDFData)0, 0,
                              inputData.bakedGI, Occlusion, inputData.positionWS,
                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);

				color.a = Alpha;

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				return PackGBuffersBRDFData(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }
			Offset[_OffsetFactor],[_OffsetUnit]

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

			#define SCENESELECTIONPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction(Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord2.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				output.ase_texcoord1.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord2 = input.ase_texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag( PackedVaryings input
				#if defined( ASE_DEPTH_WRITE_ON )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				float2 uv_MainTex = input.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord1.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord1.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord2.xyz;
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				float2 uv_TextureSample3 = input.ase_texcoord1.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord1.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord1.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord1.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord1.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				surfaceDescription.Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = 0.5;
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				return half4( _ObjectId, _PassValue, 1.0, 1.0 );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }
			Offset[_OffsetFactor],[_OffsetUnit]

			AlphaToMask Off

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

		    #define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_WORLD_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output;
				ZERO_INITIALIZE(PackedVaryings, output);

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord2.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord3.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord4.xyz = ase_bitangentWS;
				
				output.ase_texcoord1.xy = input.ase_texcoord.xy;
				output.ase_texcoord1.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord2.w = 0;
				output.ase_texcoord3.w = 0;
				output.ase_texcoord4.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.tangentOS = input.tangentOS;
				output.ase_texcoord = input.ase_texcoord;
				output.ase_texcoord2 = input.ase_texcoord2;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				output.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				output.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag( PackedVaryings input
				#if defined( ASE_DEPTH_WRITE_ON )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				float2 uv_MainTex = input.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord1.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord1.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord2.xyz;
				float3 ase_normalWS = input.ase_texcoord3.xyz;
				float3 ase_bitangentWS = input.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				float2 uv_TextureSample3 = input.ase_texcoord1.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord1.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord1.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord1.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord1.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord1.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				surfaceDescription.Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					surfaceDescription.AlphaClipThreshold = 0.5;
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				return unity_SelectionID;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "MotionVectors"
			Tags { "LightMode"="MotionVectors" }
			Offset[_OffsetFactor],[_OffsetUnit]

			ColorMask RG

			HLSLPROGRAM

			#define ASE_GEOMETRY
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _NORMALMAP 1
			#define ASE_VERSION 19908
			#define ASE_SRP_VERSION 170300


			#pragma vertex vert
			#pragma fragment frag

			#if defined( _SPECULAR_SETUP ) && defined( ASE_LIGHTING_SIMPLE )
				#if defined( _SPECULARHIGHLIGHTS_OFF )
					#undef _SPECULAR_COLOR
				#else
					#define _SPECULAR_COLOR
				#endif
			#endif

            #define SHADERPASS SHADERPASS_MOTION_VECTORS

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
		    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"

			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_TEXTURE_COORDINATES2
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
			#pragma shader_feature _IGNORESECONDROADALPHA_ON
			#pragma shader_feature _IGNORECROSSROADALPHA_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 positionOld : TEXCOORD4;
				#if _ADD_PRECOMPUTED_VELOCITY
					float3 alembicMotionVector : TEXCOORD5;
				#endif
				half3 normalOS : NORMAL;
				half4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float4 positionCSNoJitter : TEXCOORD0;
				float4 previousPositionCSNoJitter : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample4_ST;
			float4 _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _CrossRoadNormal_ST;
			float4 _TextureSample1_ST;
			float4 _DetailMask_ST;
			float4 _CrossRoadColor;
			float4 _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST;
			float4 _SecondRoadNormal_ST;
			float4 _TextureSample3_ST;
			float4 _SecondRoadColor;
			float4 _MainRoadColor;
			float4 _MainTex_ST;
			float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
			float4 _BumpMap_ST;
			float _SecondRoadAmbientOcclusionPower;
			float _SecondRoadNormalBlend;
			float _MainRoadAlphaCutOut;
			float _CrossRoadNormalScale;
			float _MainRoadMetalicPower;
			float _CrossRoadMetallicPower;
			float _MainRoadAmbientOcclusionPower;
			float _MainRoadSmoothnessPower;
			float _SecondRoadSmoothnessPower;
			float _CrossRoadAmbientOcclusionPower;
			float _CrossRoadSmoothnessPower;
			float _SecondRoadMetalicPower;
			float _SecondRoadNormalScale;
			float _MainRoadBrightness;
			float _Float3;
			float _CrossRoadParallaxPower;
			float _SecondRoadAlphaCutOut;
			float _CrossRoadUV3;
			float _CrossRoadBrightness;
			float _SecondRoadNoiseMaskTreshold;
			float _SecondRoadNoiseMaskPower;
			float _Float2;
			float _SecondRoadParallaxPower;
			float _SecondRoadUV3;
			float _SecondRoadBrightness;
			float _MainRoadParallaxPower;
			float _MainRoadUV3;
			float _BumpScale;
			float _CrossRoadAlphaCutOut;
			float _AlphaClip;
			float _Cutoff;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
			sampler2D _TextureSample3;
			sampler2D _SecondRoadNormal;
			sampler2D _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA;
			sampler2D _TextureSample1;
			sampler2D _TextureSample4;
			sampler2D _CrossRoadNormal;
			sampler2D _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA;


			
			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 ase_tangentWS = TransformObjectToWorldDir( input.tangentOS.xyz );
				output.ase_texcoord4.xyz = ase_tangentWS;
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				output.ase_texcoord5.xyz = ase_normalWS;
				float ase_tangentSign = input.tangentOS.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				output.ase_texcoord6.xyz = ase_bitangentWS;
				
				output.ase_texcoord3.xy = input.ase_texcoord.xy;
				output.ase_texcoord3.zw = input.ase_texcoord2.xy;
				output.ase_color = input.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord4.w = 0;
				output.ase_texcoord5.w = 0;
				output.ase_texcoord6.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				#if defined(APPLICATION_SPACE_WARP_MOTION)
					output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.positionOS));
					output.positionCS = output.positionCSNoJitter;
				#else
					output.positionCS = vertexInput.positionCS;
					output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.positionOS));
				#endif

				float4 prevPos = ( unity_MotionVectorsParams.x == 1 ) ? float4( input.positionOld, 1 ) : input.positionOS;

				#if _ADD_PRECOMPUTED_VELOCITY
					prevPos = prevPos - float4(input.alembicMotionVector, 0);
				#endif

				output.previousPositionCSNoJitter = mul( _PrevViewProjMatrix, mul( UNITY_PREV_MATRIX_M, prevPos ) );

				output.positionWS = vertexInput.positionWS;

				// removed in ObjectMotionVectors.hlsl found in unity 6000.0.23 and higher
				//ApplyMotionVectorZBias( output.positionCS );
				return output;
			}

			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}

			half4 frag(	PackedVaryings input
				#if defined( ASE_DEPTH_WRITE_ON )
				,out float outputDepth : ASE_SV_DEPTH
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float3 PositionWS = input.positionWS;
				float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
				float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
				float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;

				float2 uv_MainTex = input.ase_texcoord3.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv3_BumpMap = input.ase_texcoord3.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
				float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = input.ase_texcoord3.xy * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
				float3 ase_tangentWS = input.ase_texcoord4.xyz;
				float3 ase_normalWS = input.ase_texcoord5.xyz;
				float3 ase_bitangentWS = input.ase_texcoord6.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 ase_viewVectorTS =  tanToWorld0 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).x + tanToWorld1 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).y  + tanToWorld2 * ( ( unity_OrthoParams.w == 0 ) ? _WorldSpaceCameraPos - PositionWS : UNITY_MATRIX_V[ 2 ].xyz ).z;
				float3 ase_viewDirTS = normalize( ase_viewVectorTS );
				float2 Offset968 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + (( _MainRoadUV3 )?( uv3_BumpMap ):( uv_MainTex ));
				float2 Offset976 = ( ( 0.0 - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset968;
				float2 Offset986 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset976 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset976;
				float2 Offset996 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset986 ).b - 1 ) * ase_viewDirTS.xy * _MainRoadParallaxPower ) + Offset986;
				float4 tex2DNode1 = tex2D( _MainTex, Offset996 );
				float temp_output_629_0 = ( tex2DNode1.a * _MainRoadAlphaCutOut );
				float2 uv_TextureSample3 = input.ase_texcoord3.xy * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
				float2 uv3_SecondRoadNormal = input.ase_texcoord3.zw * _SecondRoadNormal_ST.xy + _SecondRoadNormal_ST.zw;
				float2 uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord3.xy * _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset888 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + (( _SecondRoadUV3 )?( uv3_SecondRoadNormal ):( uv_TextureSample3 ));
				float2 Offset909 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset888 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset888;
				float2 Offset933 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset909 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset909;
				float2 Offset957 = ( ( tex2D( _SecondRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset933 ).b - 1 ) * ase_viewDirTS.xy * _SecondRoadParallaxPower ) + Offset933;
				float4 tex2DNode537 = tex2D( _TextureSample3, Offset957 );
				#ifdef _IGNORESECONDROADALPHA_ON
				float staticSwitch693 = temp_output_629_0;
				#else
				float staticSwitch693 = ( tex2DNode537.a * _SecondRoadAlphaCutOut );
				#endif
				float4 break666 = ( input.ase_color / float4( 1,1,1,1 ) );
				float2 uv_TextureSample1 = input.ase_texcoord3.xy * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
				float temp_output_682_0 = min( min( tex2D( _TextureSample1, uv_TextureSample1 ).r, tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.4,0.4 ) ) ).r ), tex2D( _TextureSample1, ( uv_TextureSample1 * float2( 0.2,0.2 ) ) ).r );
				float clampResult673 = clamp( pow( abs( ( temp_output_682_0 * _SecondRoadNoiseMaskPower ) ) , _SecondRoadNoiseMaskTreshold ) , 0.0 , 1.0 );
				float4 appendResult665 = (float4(( ( 1.0 - break666.r ) - clampResult673 ) , ( 1.0 - break666.g ) , break666.b , break666.a));
				float4 clampResult672 = clamp( appendResult665 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 break496 = ( 1.0 - clampResult672 );
				float lerpResult628 = lerp( temp_output_629_0 , staticSwitch693 , break496.x);
				float2 uv_TextureSample4 = input.ase_texcoord3.xy * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
				float2 uv3_CrossRoadNormal = input.ase_texcoord3.zw * _CrossRoadNormal_ST.xy + _CrossRoadNormal_ST.zw;
				float2 uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA = input.ase_texcoord3.xy * _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.xy + _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA_ST.zw;
				float2 Offset889 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, uv_CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + (( _CrossRoadUV3 )?( uv3_CrossRoadNormal ):( uv_TextureSample4 ));
				float2 Offset908 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset889 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset889;
				float2 Offset932 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset908 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset908;
				float2 Offset956 = ( ( tex2D( _CrossRoadMetallicRAmbientocclusionGHeightBSmoothnessA, Offset932 ).b - 1 ) * ase_viewDirTS.xy * _CrossRoadParallaxPower ) + Offset932;
				float4 tex2DNode638 = tex2D( _TextureSample4, Offset956 );
				#ifdef _IGNORECROSSROADALPHA_ON
				float staticSwitch696 = lerpResult628;
				#else
				float staticSwitch696 = ( tex2DNode638.a * _CrossRoadAlphaCutOut );
				#endif
				float lerpResult849 = lerp( staticSwitch696 , staticSwitch693 , break496.x);
				float lerpResult641 = lerp( lerpResult628 , lerpResult849 , break496.y);
				

				float Alpha = lerpResult641;
				#if defined( _ALPHATEST_ON )
					float AlphaClipThreshold = 0.5;
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					input.positionCS.z = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#if defined(ASE_CHANGES_WORLD_POS)
					float3 positionOS = mul( GetWorldToObjectMatrix(),  float4( PositionWS, 1.0 ) ).xyz;
					float3 previousPositionWS = mul( GetPrevObjectToWorldMatrix(),  float4( positionOS, 1.0 ) ).xyz;
					input.positionCSNoJitter = mul( _NonJitteredViewProjMatrix, float4( PositionWS, 1.0 ) );
					input.previousPositionCSNoJitter = mul( _PrevViewProjMatrix, float4( previousPositionWS, 1.0 ) );
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#if defined( ASE_DEPTH_WRITE_ON )
					outputDepth = input.positionCS.z;
				#endif

				#if defined(APPLICATION_SPACE_WARP_MOTION)
					return float4( CalcAswNdcMotionVectorFromCsPositions( input.positionCSNoJitter, input.previousPositionCSNoJitter ), 1 );
				#else
					return float4( CalcNdcMotionVectorFromCsPositions( input.positionCSNoJitter, input.previousPositionCSNoJitter ), 0, 0 );
				#endif
			}
			ENDHLSL
		}

	
	}
	
	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}