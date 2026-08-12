Shader "PSX/DitheredTransparency"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha (Transparency)", Range(0.0, 1.0)) = 0.5
        _SnapResolution ("Grid Snap Resolution (Pixels)", Range(32.0, 1024.0)) = 240.0
        _JitterIntensity ("Jitter Intensity", Range(0.0, 1.0)) = 1.0
        [Toggle(_UNLIT_MODE)] _UnlitMode ("Unlit Mode (Ignorar Luz)", Float) = 1.0

        [Header(Distance Fog)]
        [Toggle(_FOG_ON)] _FogEnabled ("Enable Fog", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }

        LOD 100
        Cull Off

        Pass
        {
            Name "PSXDitheredTransparencyPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _UNLIT_MODE
            #pragma shader_feature_local _FOG_ON

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
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float  fogFactor  : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Alpha;
                float _SnapResolution;
                float _JitterIntensity;
            CBUFFER_END

            // Uniforms globais de fog publicados pelo PSXPostProcessRenderFeature via Shader.SetGlobal
            // Controlados pelo PSXVolume (secao Distance Fog) - afetam todos os materiais PSX da cena
            float  _FogGlobalEnabled;
            float4 _FogColor;
            float  _FogStart;
            float  _FogEnd;
            float  _FogDensity;
            float  _FogExponential;

            // Matriz Bayer 4x4 para mascara de transparencia estilo PS1
            static const float4x4 BAYER_4X4 = float4x4(
                 0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                 3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
                15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float4 clipPos = TransformObjectToHClip(input.positionOS.xyz);

                // PS1 Vertex Snap
                if (clipPos.w != 0.0 && _JitterIntensity > 0.001)
                {
                    float2 screenPos = clipPos.xy / clipPos.w;
                    float2 snappedPos = floor(screenPos * _SnapResolution + 0.5) / _SnapResolution;
                    clipPos.xy = lerp(clipPos.xy, snappedPos * clipPos.w, saturate(_JitterIntensity));
                }

                output.positionCS = clipPos;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // Fog calculated per vert (master enable via PSXVolume _FogGlobalEnabled, toggle per material via _FOG_ON)
                #if defined(_FOG_ON)
                    if (_FogGlobalEnabled > 0.5)
                    {
                        float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                        float dist = length(worldPos - _WorldSpaceCameraPos.xyz);
                        if (_FogExponential > 0.5)
                            output.fogFactor = 1.0 - saturate(exp(-_FogDensity * dist));
                        else
                            output.fogFactor = saturate((dist - _FogStart) / max(_FogEnd - _FogStart, 0.001));
                    }
                    else
                    {
                        output.fogFactor = 0.0;
                    }
                #else
                    output.fogFactor = 0.0;
                #endif

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Mascara de transparencia via clip() com matriz Bayer 4x4
                // Imita a transparencia por dithering do PS1 (Silent Hill, Spyro, agua, fumaca)
                uint2 pixelPos = (uint2)input.positionCS.xy;
                float bayer = BAYER_4X4[pixelPos.x % 4][pixelPos.y % 4];

                // Descarta o pixel se o valor Bayer for maior que o alpha
                // Alpha = 0.0 = completamente invisivel | Alpha = 1.0 = totalmente opaco
                clip(bayer - (1.0 - _Alpha) - 0.001);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 albedo = texColor.rgb * _Color.rgb;

                #if defined(_UNLIT_MODE)
                    half3 lighting = half3(1, 1, 1);
                #else
                    Light mainLight = GetMainLight();
                    half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                    half3 lighting = mainLight.color * NdotL + SampleSH(input.normalWS);
                    lighting = max(lighting, half3(0.4, 0.4, 0.4));
                #endif

                half3 finalColor = albedo * lighting;

                // Distance Fog: blend para FogColor global (PSXVolume) conforme distancia da camera
                #if defined(_FOG_ON)
                    finalColor = lerp(finalColor, _FogColor.rgb, input.fogFactor);
                #endif

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
