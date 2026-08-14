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

            // URP Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS

            // Lightmap Keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float3 normalOS         : NORMAL;
                float2 uv               : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS      : SV_POSITION;
                float2 uv              : TEXCOORD0;
                float3 normalWS        : TEXCOORD1;
                float  fogFactor       : TEXCOORD2;
                float3 positionWS      : TEXCOORD3;
                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                half3  vertexLighting  : TEXCOORD4;
                #endif
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
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

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.positionWS = worldPos;

                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                output.vertexLighting = VertexLighting(worldPos, output.normalWS);
                #endif

                // Fog calculated per vert (master enable via PSXVolume _FogGlobalEnabled, toggle per material via _FOG_ON)
                #if defined(_FOG_ON)
                    if (_FogGlobalEnabled > 0.5)
                    {
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
                    float3 normalWS = normalize(input.normalWS);
                    float3 positionWS = input.positionWS;

                    half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);

                    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
                        half4 shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
                    #elif !defined(LIGHTMAP_ON)
                        half4 shadowMask = unity_ProbesOcclusion;
                    #else
                        half4 shadowMask = half4(1, 1, 1, 1);
                    #endif

                    #if defined(_LIGHT_LAYERS)
                        uint meshRenderingLayers = GetMeshRenderingLayer();
                    #endif

                    // Main Light
                    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
                    Light mainLight = GetMainLight(shadowCoord, positionWS, shadowMask);

                    half3 lighting = bakedGI;

                    #if defined(_LIGHT_LAYERS)
                    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
                    #endif
                    {
                        half NdotL = saturate(dot(normalWS, mainLight.direction));
                        lighting += mainLight.color * (NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                    }

                    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                    lighting += input.vertexLighting;
                    #endif

                    // Additional Lights (Point Lights, Spotlights, Flashlight, Forward+)
                    #if defined(_ADDITIONAL_LIGHTS) || defined(_FORWARD_PLUS)
                    InputData inputData = (InputData)0;
                    inputData.positionWS = positionWS;
                    inputData.positionCS = input.positionCS;
                    inputData.normalWS = normalWS;
                    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                    inputData.shadowMask = shadowMask;

                    uint pixelLightCount = GetAdditionalLightsCount();
                    LIGHT_LOOP_BEGIN(pixelLightCount)
                        Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
                        #if defined(_LIGHT_LAYERS)
                        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                        #endif
                        {
                            half addNdotL = saturate(dot(normalWS, light.direction));
                            lighting += light.color * (addNdotL * light.distanceAttenuation * light.shadowAttenuation);
                        }
                    LIGHT_LOOP_END
                    #endif
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

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowAttributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct ShadowVaryings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Alpha;
                float _SnapResolution;
                float _JitterIntensity;
            CBUFFER_END

            static const float4x4 BAYER_4X4 = float4x4(
                 0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                 3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
                15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            float3 _LightDirection;
            float3 _LightPosition;

            ShadowVaryings ShadowPassVertex(ShadowAttributes input)
            {
                ShadowVaryings output;

                float4 clipPos = TransformObjectToHClip(input.positionOS.xyz);
                if (clipPos.w != 0.0 && _JitterIntensity > 0.001)
                {
                    float2 screenPos = clipPos.xy / clipPos.w;
                    float2 snappedPos = floor(screenPos * _SnapResolution + 0.5) / _SnapResolution;
                    clipPos.xy = lerp(clipPos.xy, snappedPos * clipPos.w, saturate(_JitterIntensity));
                }

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 ShadowPassFragment(ShadowVaryings input) : SV_TARGET
            {
                uint2 pixelPos = (uint2)input.positionCS.xy;
                float bayer = BAYER_4X4[pixelPos.x % 4][pixelPos.y % 4];
                clip(bayer - (1.0 - _Alpha) - 0.001);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Off

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DepthAttributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct DepthVaryings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Alpha;
                float _SnapResolution;
                float _JitterIntensity;
            CBUFFER_END

            static const float4x4 BAYER_4X4 = float4x4(
                 0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                 3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
                15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            DepthVaryings DepthOnlyVertex(DepthAttributes input)
            {
                DepthVaryings output;

                float4 clipPos = TransformObjectToHClip(input.positionOS.xyz);
                if (clipPos.w != 0.0 && _JitterIntensity > 0.001)
                {
                    float2 screenPos = clipPos.xy / clipPos.w;
                    float2 snappedPos = floor(screenPos * _SnapResolution + 0.5) / _SnapResolution;
                    clipPos.xy = lerp(clipPos.xy, snappedPos * clipPos.w, saturate(_JitterIntensity));
                }

                output.positionCS = clipPos;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 DepthOnlyFragment(DepthVaryings input) : SV_TARGET
            {
                uint2 pixelPos = (uint2)input.positionCS.xy;
                float bayer = BAYER_4X4[pixelPos.x % 4][pixelPos.y % 4];
                clip(bayer - (1.0 - _Alpha) - 0.001);
                return 0;
            }
            ENDHLSL
        }
    }
}
