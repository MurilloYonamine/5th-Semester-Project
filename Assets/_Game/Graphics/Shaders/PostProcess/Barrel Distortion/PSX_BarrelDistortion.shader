Shader "PSX/BarrelDistortion"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _Strength ("Distortion Strength", Range(-1.0, 1.0)) = 0.15
        _Tightness ("Tightness", Range(0.1, 10.0)) = 3.0
        _Zoom ("Zoom", Range(0.5, 2.0)) = 0.98
        _Vignette ("Corner Darkness", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        LOD 100
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "PSXBarrelDistortionPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _Tightness;
                float _Zoom;
                float _Vignette;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 uv = input.texcoord - 0.5;

                // Correcao de aspect ratio: calcula o raio em espaco de pixels reais
                // Sem isso, o raio fica oval em telas nao-quadradas (ex: 16:9, 4:3)
                float2 aspectUV = uv * float2(aspect, 1.0);
                float r2 = dot(aspectUV, aspectUV);
                
                // Formula de distorcao de lente (Olho de peixe CRT)
                float distortion = 1.0 + _Strength * pow(abs(r2), _Tightness * 0.5);
                float2 distUV = (uv * distortion) * _Zoom + 0.5;

                if (distUV.x < 0.0 || distUV.x > 1.0 || distUV.y < 0.0 || distUV.y > 1.0)
                {
                    return half4(0, 0, 0, 1);
                }

                // Mascara de borda (vignette escuro nas extremidades do CRT)
                float edgeX = smoothstep(0.0, 0.05, distUV.x) * smoothstep(1.0, 0.95, distUV.x);
                float edgeY = smoothstep(0.0, 0.05, distUV.y) * smoothstep(1.0, 0.95, distUV.y);
                float edgeMask = lerp(1.0, edgeX * edgeY, _Vignette);

                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distUV);
                return col * edgeMask;
            }
            ENDHLSL
        }
    }
}
