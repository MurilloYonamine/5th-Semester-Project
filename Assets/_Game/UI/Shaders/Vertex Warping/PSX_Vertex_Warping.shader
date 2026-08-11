Shader "PSX/Vertex_Warping"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        [HideInInspector] _BaseMap ("Base Map", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        [HideInInspector] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _SnapResolution ("Grid Snap Resolution (Pixels)", Range(32.0, 1024.0)) = 240.0
        _JitterIntensity ("Jitter Intensity", Range(0.0, 2.0)) = 1.0
        [Toggle(_UNLIT_MODE)] _UnlitMode ("Unlit Mode (Ignorar Luz)", Float) = 0.0
        [Toggle(_AFFINE_MAPPING)] _AffineMapping ("Distorcao Afim (PS1 Style)", Float) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry"
        }

        LOD 100
        Cull Back

        Pass
        {
            Name "PSXVertexWarpingPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _UNLIT_MODE
            #pragma shader_feature_local _AFFINE_MAPPING

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 uv_affine  : TEXCOORD0; // xy: UV * w, w: depth (w)
                float3 normalWS   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _BaseColor;
                float _SnapResolution;
                float _JitterIntensity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                // Transformacao de coordenadas de objeto para clip space
                float4 clipPos = TransformObjectToHClip(input.positionOS.xyz);

                // PS1 Vertex Jitter / Quantizacao de Vertices no Espaco de Tela
                if (clipPos.w != 0.0 && _JitterIntensity > 0.0)
                {
                    float2 screenPos = clipPos.xy / clipPos.w;
                    float2 grid = _SnapResolution * (_ScreenParams.xy / _ScreenParams.y);
                    
                    screenPos = floor(screenPos * grid + 0.5) / grid;
                    clipPos.xy = lerp(clipPos.xy, screenPos * clipPos.w, _JitterIntensity);
                }

                output.positionCS = clipPos;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                float2 uvScaled = TRANSFORM_TEX(input.uv, _MainTex);
                output.uv_affine = float4(uvScaled * clipPos.w, 0.0, clipPos.w);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv_affine.xy / max(input.uv_affine.w, 0.0001);
                
                #if defined(_AFFINE_MAPPING)
                    uv = input.uv_affine.xy;
                #endif

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half3 albedo = texColor.rgb * _Color.rgb * _BaseColor.rgb;
                
                #if defined(_UNLIT_MODE)
                    half3 lighting = half3(1, 1, 1);
                #else
                    Light mainLight = GetMainLight();
                    half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                    half3 lighting = mainLight.color * NdotL + SampleSH(input.normalWS);
                    // Iluminacao ambiente minima para evitar modelos pretos na ausencia de luzes
                    lighting = max(lighting, half3(0.4, 0.4, 0.4));
                #endif

                return half4(albedo * lighting, 1.0);
            }
            ENDHLSL
        }
    }
}
