
Shader "Hidden/Crepuscular" 
{
	Properties { [HideInInspector] _MainTex("Main Texture",2D) = "white" {}}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"		 
			#pragma vertex vert
			#pragma fragment frag

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float3 _LightPos;
			float _NumSamples;
			float _Density;
			float _Weight;
			float _Decay;
			float _Exposure;
			float _IlluminationDecay;
			float _AutoPoseZ;

			struct Attributes
			{
			  float4 positionOS : POSITION;
			  float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
			  float2 uv : TEXCOORD0;
			  float4 vertex : SV_POSITION;
			  UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				output.vertex = vertexInput.positionCS;
				output.uv = input.uv;
				return output;
			}

			float4 frag(Varyings i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				float2 deltaTexCoord = (i.uv - _LightPos.xy) * (_LightPos.z < 0 ? -1 : 1);
				deltaTexCoord *= 1.0f / _NumSamples * _Density;
				float2 uv = i.uv;
				float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).xyz;

				for (int i = 0; i < (_LightPos.z < 0 ? 0 : _NumSamples * _LightPos.z); i++)
				{
						uv -= deltaTexCoord;
						float3 sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).xyz;
						sample *= _IlluminationDecay * (_Weight / _NumSamples);
						color += sample;
						_IlluminationDecay *= _Decay;		
					
				}
				return float4(color * _Exposure, 1);
			}
			ENDHLSL
		}
	}
}