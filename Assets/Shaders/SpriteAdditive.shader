Shader "Sprites/Additive"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline"="UniversalPipeline"
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One One

		HLSLPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment SpriteFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
				half4 _Color;
				half4 _RendererColor;
				float4 _Flip;
				float4 _MainTex_ST;
			CBUFFER_END

			struct appdata
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				half4 color     : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f SpriteVert(appdata IN)
			{
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			#ifdef UNITY_INSTANCING_ENABLED
				IN.vertex.xy *= _Flip.xy;
			#endif

				OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.color = IN.color * _Color * _RendererColor;

			#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
			#endif

				return OUT;
			}

			half4 SpriteFrag(v2f IN) : SV_Target
			{
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord);
				half4 c = texColor * IN.color;
				c.rgb *= c.a;
				return c;
			}

		ENDHLSL
		}
	}

	Fallback "Sprites/Default"
}
