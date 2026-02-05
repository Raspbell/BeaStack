// Made with Amplify Shader Editor v1.9.9.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Archanor VFX/Retro Arsenal/2D/2DPowerbox"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_Background( "Background", 2D ) = "white" {}
		_PanningTexture( "Panning Texture", 2D ) = "white" {}
		_Icon( "Icon", 2D ) = "white" {}
		[Header(Background  Edge Settings)][Space(10)] _BackgroundTint( "Background Tint", Color ) = ( 0, 0, 0, 0 )
		_EdgeTint( "Edge Tint", Color ) = ( 0, 0, 0, 0 )
		[Toggle( _GAMMATOLINEAR_ON )] _GammatoLinear( "Gamma to Linear", Float ) = 0
		_BackgroundPower( "Background Power", Float ) = 1
		_EdgePower( "Edge Power", Float ) = 1
		_Vector1( "Vector 1", Vector ) = ( 0, 0, 1, 0 )
		_Contrast( "Contrast", Float ) = 0
		[Header(Icon Settings)][Space(10)] _IconTint( "Icon Tint", Color ) = ( 0, 0, 0, 0 )
		_PanColor( "Pan Color", Color ) = ( 0.7028302, 0.8790489, 1, 0 )
		_IconContrast( "Icon Contrast", Range( 1, 1.5 ) ) = 0
		_IconEmission( "Icon Emission", Range( 1, 15 ) ) = 0
		_PanStrength( "Pan Strength", Range( 1, 5 ) ) = 1
		[Header(Rotation)][Space(10)] _RotationSpeed( "Rotation Speed", Float ) = 5
		[Header(Bobbing)][Space(10)] _BobSpeed( "Bob Speed", Float ) = 4
		_BobHeight( "Bob Height", Range( 0, 0.2 ) ) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		[HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off

		HLSLINCLUDE
		#pragma target 2.0
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		ENDHLSL

		
		Pass
		{
			Name "Sprite Unlit"
            Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZTest LEqual
			ZWrite On
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19905
			#define ASE_SRP_VERSION 170003


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
            #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX

			#define SHADERPASS SHADERPASS_SPRITEUNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _GAMMATOLINEAR_ON


			sampler2D _Background;
			sampler2D _Icon;
			sampler2D _PanningTexture;
			CBUFFER_START( UnityPerMaterial )
			float4 _BackgroundTint;
			float4 _Background_ST;
			float4 _EdgeTint;
			float4 _IconTint;
			float4 _PanColor;
			float3 _Vector1;
			float _RotationSpeed;
			float _BobSpeed;
			float _BobHeight;
			float _Contrast;
			float _BackgroundPower;
			float _EdgePower;
			float _IconContrast;
			float _IconEmission;
			float _PanStrength;
			CBUFFER_END


			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_SKINNED_VERTEX_INPUTS
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D(_AlphaTex); SAMPLER(sampler_AlphaTex);
				float _EnableAlphaTexture;
			#endif

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			
			float4 CalculateContrast( float contrastValue, float4 colorTarget )
			{
				float t = 0.5 * ( 1.0 - contrastValue );
				return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
			}

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_SKINNED_VERTEX_COMPUTE( v );

				v.positionOS = UnityFlipSprite( v.positionOS, unity_SpriteProps.xy );

				float mulTime76 = _TimeParameters.x * _RotationSpeed;
				float3 temp_cast_0 = (0.0).xxx;
				float4 appendResult276 = (float4(0.0 , 0.0 , _BobHeight , 0.0));
				float3 rotatedValue393 = RotateAroundAxis( temp_cast_0, ( v.positionOS + (v.positionOS*0.0 + ( sin( ( _TimeParameters.x * _BobSpeed ) ) * appendResult276 ).xyz) ), normalize( _Vector1 ), mulTime76 );
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = rotatedValue393;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS = vertexValue;
				#else
					v.positionOS += vertexValue;
				#endif
				v.normal = v.normal;
				v.tangent.xyz = v.tangent.xyz;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS);

				o.positionCS = vertexInput.positionCS;
				o.positionWS = vertexInput.positionWS;
				o.texCoord0 = v.uv0;
				o.color = v.color;
				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				float4 positionCS = IN.positionCS;
				float3 positionWS = IN.positionWS;

				float2 uv_Background = IN.texCoord0.xy * _Background_ST.xy + _Background_ST.zw;
				float4 tex2DNode9 = tex2D( _Background, uv_Background );
				float temp_output_321_0 = ( 1.0 - tex2DNode9.a );
				float2 temp_cast_1 = (2.0).xx;
				float2 texCoord83 = IN.texCoord0.xy * temp_cast_1 + float3( 0, -1, 0 ).xy;
				float4 tex2DNode84 = tex2D( _Icon, texCoord83 );
				float2 temp_cast_4 = (1.0).xx;
				float2 texCoord101 = IN.texCoord0.xy * temp_cast_4 + float2( 0,0 );
				float2 panner95 = ( _TimeParameters.x * float2( 0,1.5 ) + texCoord101);
				float2 temp_cast_6 = (2.0).xx;
				float2 texCoord186 = IN.texCoord0.xy * temp_cast_6 + float2( 0,0 );
				float2 panner184 = ( _TimeParameters.x * float2( 0,2 ) + texCoord186);
				float3 panColor278 = _PanColor.rgb;
				float4 pan2280 = ( tex2D( _PanningTexture, panner184 ) * 0.5 * float4( panColor278 , 0.0 ) );
				float4 lerpResult87 = lerp( CalculateContrast(_Contrast,float4( ( ( _BackgroundTint.rgb * tex2DNode9.a * _BackgroundPower ) + tex2DNode9.rgb + ( temp_output_321_0 * _EdgeTint.rgb * _EdgePower ) ) , 0.0 )) , ( CalculateContrast(_IconContrast,float4( ( tex2DNode84.r * _IconEmission * _IconTint.rgb ) , 0.0 )) + ( float4( ( tex2D( _PanningTexture, panner95 ).rgb * _PanStrength * _PanColor.rgb ) , 0.0 ) + pan2280 ) ) , tex2DNode84.a);
				float3 gammaToLinear481 = FastSRGBToLinear( lerpResult87.rgb );
				#ifdef _GAMMATOLINEAR_ON
				float4 staticSwitch479 = float4( gammaToLinear481 , 0.0 );
				#else
				float4 staticSwitch479 = lerpResult87;
				#endif
				float4 appendResult4_g1 = (float4(staticSwitch479.rgb , _BackgroundTint.a));
				
				float4 Color = appendResult4_g1;

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, IN.texCoord0.xy);
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture);
				#endif

				#if defined(DEBUG_DISPLAY)
				SurfaceData2D surfaceData;
				InitializeSurfaceData(Color.rgb, Color.a, surfaceData);
				InputData2D inputData;
				InitializeInputData(positionWS.xy, half2(IN.texCoord0.xy), inputData);
				half4 debugColor = 0;

				SETUP_DEBUG_DATA_2D(inputData, positionWS, positionCS);

				if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
				{
					return debugColor;
				}
				#endif

				Color *= IN.color * unity_SpriteColor;
				return Color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
            Name "Sprite Unlit Forward"
            Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZTest LEqual
			ZWrite On
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19905
			#define ASE_SRP_VERSION 170003


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
            #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX

			#define SHADERPASS SHADERPASS_SPRITEFORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _GAMMATOLINEAR_ON


			sampler2D _Background;
			sampler2D _Icon;
			sampler2D _PanningTexture;
			CBUFFER_START( UnityPerMaterial )
			float4 _BackgroundTint;
			float4 _Background_ST;
			float4 _EdgeTint;
			float4 _IconTint;
			float4 _PanColor;
			float3 _Vector1;
			float _RotationSpeed;
			float _BobSpeed;
			float _BobHeight;
			float _Contrast;
			float _BackgroundPower;
			float _EdgePower;
			float _IconContrast;
			float _IconEmission;
			float _PanStrength;
			CBUFFER_END


			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_SKINNED_VERTEX_INPUTS
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			
			float4 CalculateContrast( float contrastValue, float4 colorTarget )
			{
				float t = 0.5 * ( 1.0 - contrastValue );
				return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
			}

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_SKINNED_VERTEX_COMPUTE( v );

				v.positionOS = UnityFlipSprite( v.positionOS, unity_SpriteProps.xy );

				float mulTime76 = _TimeParameters.x * _RotationSpeed;
				float3 temp_cast_0 = (0.0).xxx;
				float4 appendResult276 = (float4(0.0 , 0.0 , _BobHeight , 0.0));
				float3 rotatedValue393 = RotateAroundAxis( temp_cast_0, ( v.positionOS + (v.positionOS*0.0 + ( sin( ( _TimeParameters.x * _BobSpeed ) ) * appendResult276 ).xyz) ), normalize( _Vector1 ), mulTime76 );
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS;
				#else
					float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = rotatedValue393;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS = vertexValue;
				#else
					v.positionOS += vertexValue;
				#endif
				v.normal = v.normal;
				v.tangent.xyz = v.tangent.xyz;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS);

				o.positionCS = vertexInput.positionCS;
				o.positionWS = vertexInput.positionWS;
				o.texCoord0 = v.uv0;
				o.color = v.color;

				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				float4 positionCS = IN.positionCS;
				float3 positionWS = IN.positionWS;

				float2 uv_Background = IN.texCoord0.xy * _Background_ST.xy + _Background_ST.zw;
				float4 tex2DNode9 = tex2D( _Background, uv_Background );
				float temp_output_321_0 = ( 1.0 - tex2DNode9.a );
				float2 temp_cast_1 = (2.0).xx;
				float2 texCoord83 = IN.texCoord0.xy * temp_cast_1 + float3( 0, -1, 0 ).xy;
				float4 tex2DNode84 = tex2D( _Icon, texCoord83 );
				float2 temp_cast_4 = (1.0).xx;
				float2 texCoord101 = IN.texCoord0.xy * temp_cast_4 + float2( 0,0 );
				float2 panner95 = ( _TimeParameters.x * float2( 0,1.5 ) + texCoord101);
				float2 temp_cast_6 = (2.0).xx;
				float2 texCoord186 = IN.texCoord0.xy * temp_cast_6 + float2( 0,0 );
				float2 panner184 = ( _TimeParameters.x * float2( 0,2 ) + texCoord186);
				float3 panColor278 = _PanColor.rgb;
				float4 pan2280 = ( tex2D( _PanningTexture, panner184 ) * 0.5 * float4( panColor278 , 0.0 ) );
				float4 lerpResult87 = lerp( CalculateContrast(_Contrast,float4( ( ( _BackgroundTint.rgb * tex2DNode9.a * _BackgroundPower ) + tex2DNode9.rgb + ( temp_output_321_0 * _EdgeTint.rgb * _EdgePower ) ) , 0.0 )) , ( CalculateContrast(_IconContrast,float4( ( tex2DNode84.r * _IconEmission * _IconTint.rgb ) , 0.0 )) + ( float4( ( tex2D( _PanningTexture, panner95 ).rgb * _PanStrength * _PanColor.rgb ) , 0.0 ) + pan2280 ) ) , tex2DNode84.a);
				float3 gammaToLinear481 = FastSRGBToLinear( lerpResult87.rgb );
				#ifdef _GAMMATOLINEAR_ON
				float4 staticSwitch479 = float4( gammaToLinear481 , 0.0 );
				#else
				float4 staticSwitch479 = lerpResult87;
				#endif
				float4 appendResult4_g1 = (float4(staticSwitch479.rgb , _BackgroundTint.a));
				
				float4 Color = appendResult4_g1;

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D( _AlphaTex, sampler_AlphaTex, IN.texCoord0.xy );
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture );
				#endif


				#if defined(DEBUG_DISPLAY)
					SurfaceData2D surfaceData;
					InitializeSurfaceData(Color.rgb, Color.a, surfaceData);
					InputData2D inputData;
					InitializeInputData(positionWS.xy, half2(IN.texCoord0.xy), inputData);
					half4 debugColor = 0;

					SETUP_DEBUG_DATA_2D(inputData, positionWS, positionCS);

					if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
					{
						return debugColor;
					}
				#endif

				Color *= IN.color * unity_SpriteColor;
				return Color;
			}

			ENDHLSL
		}
		
        Pass
        {
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }

            Cull Off

            HLSLPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19905
			#define ASE_SRP_VERSION 170003


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
            #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
            #define FEATURES_GRAPH_VERTEX

            #define SHADERPASS SHADERPASS_DEPTHONLY
			#define SCENESELECTIONPASS 1

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
			#pragma shader_feature_local _GAMMATOLINEAR_ON


			sampler2D _Background;
			sampler2D _Icon;
			sampler2D _PanningTexture;
			CBUFFER_START( UnityPerMaterial )
			float4 _BackgroundTint;
			float4 _Background_ST;
			float4 _EdgeTint;
			float4 _IconTint;
			float4 _PanColor;
			float3 _Vector1;
			float _RotationSpeed;
			float _BobSpeed;
			float _BobHeight;
			float _Contrast;
			float _BackgroundPower;
			float _EdgePower;
			float _IconContrast;
			float _IconEmission;
			float _PanStrength;
			CBUFFER_END


            struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_SKINNED_VERTEX_INPUTS
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            int _ObjectId;
            int _PassValue;

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			
			float4 CalculateContrast( float contrastValue, float4 colorTarget )
			{
				float t = 0.5 * ( 1.0 - contrastValue );
				return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
			}

			VertexOutput vert(VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_SKINNED_VERTEX_COMPUTE(v);

				v.positionOS = UnityFlipSprite( v.positionOS, unity_SpriteProps.xy );

				float mulTime76 = _TimeParameters.x * _RotationSpeed;
				float3 temp_cast_0 = (0.0).xxx;
				float4 appendResult276 = (float4(0.0 , 0.0 , _BobHeight , 0.0));
				float3 rotatedValue393 = RotateAroundAxis( temp_cast_0, ( v.positionOS + (v.positionOS*0.0 + ( sin( ( _TimeParameters.x * _BobSpeed ) ) * appendResult276 ).xyz) ), normalize( _Vector1 ), mulTime76 );
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = rotatedValue393;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS = vertexValue;
				#else
					v.positionOS += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS);
				float3 positionWS = TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			half4 frag(VertexOutput IN) : SV_TARGET
			{
				float2 uv_Background = IN.ase_texcoord.xy * _Background_ST.xy + _Background_ST.zw;
				float4 tex2DNode9 = tex2D( _Background, uv_Background );
				float temp_output_321_0 = ( 1.0 - tex2DNode9.a );
				float2 temp_cast_1 = (2.0).xx;
				float2 texCoord83 = IN.ase_texcoord.xy * temp_cast_1 + float3( 0, -1, 0 ).xy;
				float4 tex2DNode84 = tex2D( _Icon, texCoord83 );
				float2 temp_cast_4 = (1.0).xx;
				float2 texCoord101 = IN.ase_texcoord.xy * temp_cast_4 + float2( 0,0 );
				float2 panner95 = ( _TimeParameters.x * float2( 0,1.5 ) + texCoord101);
				float2 temp_cast_6 = (2.0).xx;
				float2 texCoord186 = IN.ase_texcoord.xy * temp_cast_6 + float2( 0,0 );
				float2 panner184 = ( _TimeParameters.x * float2( 0,2 ) + texCoord186);
				float3 panColor278 = _PanColor.rgb;
				float4 pan2280 = ( tex2D( _PanningTexture, panner184 ) * 0.5 * float4( panColor278 , 0.0 ) );
				float4 lerpResult87 = lerp( CalculateContrast(_Contrast,float4( ( ( _BackgroundTint.rgb * tex2DNode9.a * _BackgroundPower ) + tex2DNode9.rgb + ( temp_output_321_0 * _EdgeTint.rgb * _EdgePower ) ) , 0.0 )) , ( CalculateContrast(_IconContrast,float4( ( tex2DNode84.r * _IconEmission * _IconTint.rgb ) , 0.0 )) + ( float4( ( tex2D( _PanningTexture, panner95 ).rgb * _PanStrength * _PanColor.rgb ) , 0.0 ) + pan2280 ) ) , tex2DNode84.a);
				float3 gammaToLinear481 = FastSRGBToLinear( lerpResult87.rgb );
				#ifdef _GAMMATOLINEAR_ON
				float4 staticSwitch479 = float4( gammaToLinear481 , 0.0 );
				#else
				float4 staticSwitch479 = lerpResult87;
				#endif
				float4 appendResult4_g1 = (float4(staticSwitch479.rgb , _BackgroundTint.a));
				
				float4 Color = appendResult4_g1;

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}

            ENDHLSL
        }

		
        Pass
        {
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }

			Cull Off

            HLSLPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19905
			#define ASE_SRP_VERSION 170003


			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
            #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
            #define FEATURES_GRAPH_VERTEX

            #define SHADERPASS SHADERPASS_DEPTHONLY
			#define SCENEPICKINGPASS 1

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        	#define ASE_NEEDS_VERT_POSITION
        	#define ASE_NEEDS_TEXTURE_COORDINATES0
        	#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
        	#pragma shader_feature_local _GAMMATOLINEAR_ON


			sampler2D _Background;
			sampler2D _Icon;
			sampler2D _PanningTexture;
			CBUFFER_START( UnityPerMaterial )
			float4 _BackgroundTint;
			float4 _Background_ST;
			float4 _EdgeTint;
			float4 _IconTint;
			float4 _PanColor;
			float3 _Vector1;
			float _RotationSpeed;
			float _BobSpeed;
			float _BobHeight;
			float _Contrast;
			float _BackgroundPower;
			float _EdgePower;
			float _IconContrast;
			float _IconEmission;
			float _PanStrength;
			CBUFFER_END


            struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_SKINNED_VERTEX_INPUTS
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            float4 _SelectionID;

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			
			float4 CalculateContrast( float contrastValue, float4 colorTarget )
			{
				float t = 0.5 * ( 1.0 - contrastValue );
				return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
			}

			VertexOutput vert(VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_SKINNED_VERTEX_COMPUTE(v);

				v.positionOS = UnityFlipSprite( v.positionOS, unity_SpriteProps.xy );

				float mulTime76 = _TimeParameters.x * _RotationSpeed;
				float3 temp_cast_0 = (0.0).xxx;
				float4 appendResult276 = (float4(0.0 , 0.0 , _BobHeight , 0.0));
				float3 rotatedValue393 = RotateAroundAxis( temp_cast_0, ( v.positionOS + (v.positionOS*0.0 + ( sin( ( _TimeParameters.x * _BobSpeed ) ) * appendResult276 ).xyz) ), normalize( _Vector1 ), mulTime76 );
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = rotatedValue393;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS = vertexValue;
				#else
					v.positionOS += vertexValue;
				#endif

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS);
				float3 positionWS = TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				float2 uv_Background = IN.ase_texcoord.xy * _Background_ST.xy + _Background_ST.zw;
				float4 tex2DNode9 = tex2D( _Background, uv_Background );
				float temp_output_321_0 = ( 1.0 - tex2DNode9.a );
				float2 temp_cast_1 = (2.0).xx;
				float2 texCoord83 = IN.ase_texcoord.xy * temp_cast_1 + float3( 0, -1, 0 ).xy;
				float4 tex2DNode84 = tex2D( _Icon, texCoord83 );
				float2 temp_cast_4 = (1.0).xx;
				float2 texCoord101 = IN.ase_texcoord.xy * temp_cast_4 + float2( 0,0 );
				float2 panner95 = ( _TimeParameters.x * float2( 0,1.5 ) + texCoord101);
				float2 temp_cast_6 = (2.0).xx;
				float2 texCoord186 = IN.ase_texcoord.xy * temp_cast_6 + float2( 0,0 );
				float2 panner184 = ( _TimeParameters.x * float2( 0,2 ) + texCoord186);
				float3 panColor278 = _PanColor.rgb;
				float4 pan2280 = ( tex2D( _PanningTexture, panner184 ) * 0.5 * float4( panColor278 , 0.0 ) );
				float4 lerpResult87 = lerp( CalculateContrast(_Contrast,float4( ( ( _BackgroundTint.rgb * tex2DNode9.a * _BackgroundPower ) + tex2DNode9.rgb + ( temp_output_321_0 * _EdgeTint.rgb * _EdgePower ) ) , 0.0 )) , ( CalculateContrast(_IconContrast,float4( ( tex2DNode84.r * _IconEmission * _IconTint.rgb ) , 0.0 )) + ( float4( ( tex2D( _PanningTexture, panner95 ).rgb * _PanStrength * _PanColor.rgb ) , 0.0 ) + pan2280 ) ) , tex2DNode84.a);
				float3 gammaToLinear481 = FastSRGBToLinear( lerpResult87.rgb );
				#ifdef _GAMMATOLINEAR_ON
				float4 staticSwitch479 = float4( gammaToLinear481 , 0.0 );
				#else
				float4 staticSwitch479 = lerpResult87;
				#endif
				float4 appendResult4_g1 = (float4(staticSwitch479.rgb , _BackgroundTint.a));
				
				float4 Color = appendResult4_g1;
				half4 outColor = unity_SelectionID;
				return outColor;
			}

            ENDHLSL
        }
		
	}
	CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19905
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;282;-4632.791,-1161.039;Inherit;False;1809.49;636.0625;;11;280;279;187;188;185;183;189;192;191;186;184;Pan 2;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;192;-4564.957,-717.8377;Inherit;False;Constant;_Float3;Float 2;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;188;-4516.454,-1066.749;Inherit;False;Constant;_Float1;Float 0;12;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;186;-4324.652,-1121.029;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;191;-4353.352,-761.7016;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;183;-4354.652,-910.0276;Inherit;False;Constant;_Vector2;Vector 0;12;0;Create;True;0;0;0;False;0;False;0,2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;121;-2257.283,628.8722;Inherit;False;Constant;_Float2;Float 2;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;102;-2208.779,279.9617;Inherit;False;Constant;_Float0;Float 0;12;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;278;-1159.251,810.0306;Inherit;False;panColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;184;-4046.935,-992.7426;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;100;-2046.978,436.682;Inherit;False;Constant;_Vector0;Vector 0;12;0;Create;True;0;0;0;False;0;False;0,1.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;101;-2016.978,225.6822;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;98;-2045.678,585.0086;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;82;-1924.914,-88.03061;Inherit;False;Constant;_IconOffset;Icon Offset;7;0;Create;True;0;0;0;False;0;False;0,-1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;81;-1900.768,-187.4597;Inherit;False;Constant;_IconScale;Icon Scale;7;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;189;-3668.406,-740.3577;Inherit;False;Constant;_PanStrength1;Pan Strength;12;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;185;-3763.39,-981.8496;Inherit;True;Property;_BackgroundPan;BackgroundPan;1;0;Create;True;0;0;0;False;0;False;-1;None;4d64936f26b0b0e458f06562c1064b59;True;0;False;white;Auto;False;Instance;96;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;279;-3640.739,-621.4001;Inherit;False;278;panColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;95;-1739.264,353.967;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;83;-1648.23,-170.3728;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;187;-3330.665,-980.2618;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;254;-832,912;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;247;-800,1008;Inherit;False;Property;_BobSpeed;Bob Speed;16;1;[Header];Create;True;1;Bobbing;0;0;False;1;Space(10);False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;84;-1371.741,-194.1203;Inherit;True;Property;_Icon;Icon;2;0;Create;True;0;0;0;False;0;False;-1;None;f3473e6400297ed45b8303c34955caca;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;88;-1362.716,-4.57361;Inherit;False;Property;_IconEmission;Icon Emission;13;0;Create;True;0;0;0;False;0;False;0;2.5;1;15;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;110;-1499.907,623.1202;Inherit;False;Property;_PanStrength;Pan Strength;14;0;Create;True;0;0;0;False;0;False;1;1;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;112;-1416.931,793.0872;Inherit;False;Property;_PanColor;Pan Color;11;0;Create;True;0;0;0;False;0;False;0.7028302,0.8790489,1,0;0.1296362,1,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;96;-1455.718,364.8605;Inherit;True;Property;_PanningTexture;Panning Texture;1;0;Create;True;0;0;0;False;0;False;-1;None;4d64936f26b0b0e458f06562c1064b59;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;280;-3070.549,-978.2825;Inherit;False;pan2;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;9;-1899.814,-873.7063;Inherit;True;Property;_Background;Background;0;0;Create;True;0;0;0;False;0;False;-1;f92e36843399bfa4d983db282d5e8cd2;f92e36843399bfa4d983db282d5e8cd2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;326;-1284.294,-490.786;Inherit;False;Property;_EdgeTint;Edge Tint;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.1374768,0.6603774,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;308;-1250.944,-305.815;Inherit;False;Property;_EdgePower;Edge Power;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;332;-1597.888,-983.8474;Inherit;False;Property;_BackgroundPower;Background Power;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;85;-1299.118,89.29703;Inherit;False;Property;_IconTint;Icon Tint;10;1;[Header];Create;True;1;Icon Settings;0;0;False;1;Space(10);False;0,0,0,0;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;6;-1488,-1216;Inherit;False;Property;_BackgroundTint;Background Tint;3;1;[Header];Create;True;1;Background  Edge Settings;0;0;False;1;Space(10);False;0,0,0,0;0,0,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;321;-1520,-656;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;265;-848,1136;Inherit;False;Property;_BobHeight;Bob Height;17;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;255;-640,944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;93;-1054.642,-28.98039;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;109;-1095.894,502.1538;Inherit;True;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;91;-1084.111,182.5846;Inherit;False;Property;_IconContrast;Icon Contrast;12;0;Create;True;0;0;0;False;0;False;0;1.5;1;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;281;-878.39,704.4095;Inherit;False;280;pan2;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;327;-1033.04,-613.5797;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;325;-1212.517,-1042.974;Inherit;True;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;276;-512,1088;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;253;-480,960;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleContrastOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;200;-811.5876,-27.82151;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;328;-674.1044,-893.1713;Inherit;True;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;331;-454.968,-789.2693;Inherit;False;Property;_Contrast;Contrast;9;0;Create;True;0;0;0;False;0;False;0;1.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;277;-720,320;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;370;-336,832;Inherit;False;Constant;_Float5;Float 5;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;236;-304,976;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;86;-524.0716,-27.3208;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleContrastOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;330;-222.6052,-904.5055;Inherit;True;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;426;-384,608;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;69;-384,480;Inherit;False;Property;_RotationSpeed;Rotation Speed;15;1;[Header];Create;True;1;Rotation;0;0;False;1;Space(10);False;5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;230;-112,768;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;87;-210.5475,-150.4043;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GammaToLinearNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;481;80,-160;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;76;-128,480;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;73;-384,272;Inherit;False;Property;_Vector1;Vector 1;8;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;424;-96,576;Inherit;False;Constant;_Float6;Float 6;22;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;432;80,624;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;479;336,-176;Inherit;False;Property;_GammatoLinear;Gamma to Linear;5;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;474;-64,-400;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;393;192,432;Inherit;False;True;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;477;-400,-416;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;472;688,-160;Inherit;False;Alpha Merge;-1;;1;e0d79828992f19c4f90bfc29aa19b7a5;0;2;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;468;496,16;Float;False;False;-1;3;UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;Sprite Unlit Forward;0;1;Sprite Unlit Forward;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;14;all;0;False;True;2;5;False;;10;False;;3;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;469;496,16;Float;False;False;-1;3;UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;SceneSelectionPass;0;2;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;14;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;470;496,16;Float;False;False;-1;3;UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI;0;1;New Amplify Shader;cf964e524c8e69742b1d21fbe2ebcc4a;True;ScenePickingPass;0;3;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;14;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;467;880,16;Float;False;True;-1;3;UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI;0;18;Archanor VFX/Retro Arsenal/2D/2DPowerbox;cf964e524c8e69742b1d21fbe2ebcc4a;True;Sprite Unlit;0;0;Sprite Unlit;4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;True;14;all;0;False;True;2;5;False;;10;False;;3;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;3;Vertex Position;0;639031454793833921;Debug Display;0;0;External Alpha;0;639031457018322911;0;4;True;True;True;True;False;;False;0
WireConnection;186;0;188;0
WireConnection;191;0;192;0
WireConnection;278;0;112;5
WireConnection;184;0;186;0
WireConnection;184;2;183;0
WireConnection;184;1;191;0
WireConnection;101;0;102;0
WireConnection;98;0;121;0
WireConnection;185;1;184;0
WireConnection;95;0;101;0
WireConnection;95;2;100;0
WireConnection;95;1;98;0
WireConnection;83;0;81;0
WireConnection;83;1;82;0
WireConnection;187;0;185;0
WireConnection;187;1;189;0
WireConnection;187;2;279;0
WireConnection;84;1;83;0
WireConnection;96;1;95;0
WireConnection;280;0;187;0
WireConnection;321;0;9;4
WireConnection;255;0;254;0
WireConnection;255;1;247;0
WireConnection;93;0;84;1
WireConnection;93;1;88;0
WireConnection;93;2;85;5
WireConnection;109;0;96;5
WireConnection;109;1;110;0
WireConnection;109;2;112;5
WireConnection;327;0;321;0
WireConnection;327;1;326;5
WireConnection;327;2;308;0
WireConnection;325;0;6;5
WireConnection;325;1;9;4
WireConnection;325;2;332;0
WireConnection;276;2;265;0
WireConnection;253;0;255;0
WireConnection;200;1;93;0
WireConnection;200;0;91;0
WireConnection;328;0;325;0
WireConnection;328;1;9;5
WireConnection;328;2;327;0
WireConnection;277;0;109;0
WireConnection;277;1;281;0
WireConnection;236;0;253;0
WireConnection;236;1;276;0
WireConnection;86;0;200;0
WireConnection;86;1;277;0
WireConnection;330;1;328;0
WireConnection;330;0;331;0
WireConnection;230;0;426;0
WireConnection;230;1;370;0
WireConnection;230;2;236;0
WireConnection;87;0;330;0
WireConnection;87;1;86;0
WireConnection;87;2;84;4
WireConnection;481;0;87;0
WireConnection;76;0;69;0
WireConnection;432;0;426;0
WireConnection;432;1;230;0
WireConnection;479;1;87;0
WireConnection;479;0;481;0
WireConnection;474;0;6;4
WireConnection;474;1;321;0
WireConnection;393;0;73;0
WireConnection;393;1;76;0
WireConnection;393;2;424;0
WireConnection;393;3;432;0
WireConnection;472;2;479;0
WireConnection;472;3;6;4
WireConnection;467;1;472;0
WireConnection;467;3;393;0
ASEEND*/
//CHKSM=40FBBE37A7DEB21BF5EDA45EBB7D20FCD1CF62F0