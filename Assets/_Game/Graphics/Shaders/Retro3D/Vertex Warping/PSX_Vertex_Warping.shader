Shader "PSX/Vertex_Warping"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        [HideInInspector] _BaseMap ("Base Map", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        [HideInInspector] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _SnapResolution ("Grid Snap Resolution (Pixels)", Range(32.0, 1024.0)) = 240.0
        _JitterIntensity ("Jitter Intensity", Range(0.0, 1.0)) = 1.0
        [Toggle(_UNLIT_MODE)] _UnlitMode ("Unlit Mode (Ignorar Luz)", Float) = 0.0
        [Toggle(_AFFINE_MAPPING)] _AffineMapping ("Distorcao Afim (PS1 Style)", Float) = 0.0
        _AffineStrength ("Affine Strength", Range(0.0, 1.0)) = 1.0
        [Toggle(_VERTEX_COLOR)] _VertexColor ("Gouraud Vertex Color", Float) = 0.0

        [Header(Distance Fog)]
        [Toggle(_FOG_ON)] _FogEnabled ("Enable Fog", Float) = 0.0
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
            #pragma shader_feature_local _VERTEX_COLOR
            #pragma shader_feature_local _FOG_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float4 uv_affine   : TEXCOORD0; // xy: UV * w (perspective-correct), w: depth w
                float2 uvRaw       : TEXCOORD1; // UV puro para affine mapping autentico
                float3 normalWS    : TEXCOORD2;
                float4 vertexColor : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _BaseColor;
                float _SnapResolution;
                float _JitterIntensity;
                float _AffineStrength;
            CBUFFER_END

            // Uniforms globais de fog publicados pelo PSXPostProcessRenderFeature via Shader.SetGlobal
            // Controlados pelo PSXVolume (secao Distance Fog) - afetam todos os materiais PSX da cena
            float  _FogGlobalEnabled;
            float4 _FogColor;
            float  _FogStart;
            float  _FogEnd;
            float  _FogDensity;
            float  _FogExponential;

            Varyings Vert(Attributes input)
            {
                Varyings output;

                // Transformacao de coordenadas de objeto para clip space
                float4 clipPos = TransformObjectToHClip(input.positionOS.xyz);

                // PS1 Vertex Snap: quantizacao em NDC puro [-1, 1]
                // Interpola o RESULTADO final (nao a resolucao da grade) para controle preciso
                if (clipPos.w != 0.0 && _JitterIntensity > 0.001)
                {
                    float2 screenPos = clipPos.xy / clipPos.w;
                    float2 snappedPos = floor(screenPos * _SnapResolution + 0.5) / _SnapResolution;
                    clipPos.xy = lerp(clipPos.xy, snappedPos * clipPos.w, saturate(_JitterIntensity));
                }

                output.positionCS = clipPos;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.vertexColor = input.color;

                float2 uvScaled = TRANSFORM_TEX(input.uv, _MainTex);
                // uv_affine.xy = UV * w para perspective-correct divide no fragment
                output.uv_affine = float4(uvScaled * clipPos.w, 0.0, clipPos.w);
                // uvRaw = UV interpolado linearmente pelo GPU (sem correcao de perspectiva explicita)
                output.uvRaw = uvScaled;

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
                // Perspective-correct UV (padrao moderno - divide UV*w por w)
                float2 perspUV = input.uv_affine.xy / max(input.uv_affine.w, 0.0001);
                float2 uv = perspUV;

                #if defined(_AFFINE_MAPPING)
                    // Affine mapping PS1: interpola entre perspectiva-correta e UV linear puro
                    // _AffineStrength = 1.0 reproduce o warping do PS1; 0.0 = perspectiva correta moderna
                    uv = lerp(perspUV, input.uvRaw, _AffineStrength);
                #endif

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half3 albedo = texColor.rgb * _Color.rgb * _BaseColor.rgb;

                // Gouraud Shading: multiplica albedo pelas vertex colors baked no mesh
                #if defined(_VERTEX_COLOR)
                    albedo *= input.vertexColor.rgb;
                #endif

                #if defined(_UNLIT_MODE)
                    half3 lighting = half3(1, 1, 1);
                #else
                    Light mainLight = GetMainLight();
                    half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                    half3 lighting = mainLight.color * NdotL + SampleSH(input.normalWS);
                    // Iluminacao ambiente minima para evitar modelos pretos na ausencia de luzes
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
