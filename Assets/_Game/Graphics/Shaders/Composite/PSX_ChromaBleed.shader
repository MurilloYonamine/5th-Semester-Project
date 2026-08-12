Shader "PSX/ChromaBleed"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _BleedAmount ("Bleed Spread Width", Range(0.0, 0.02)) = 0.006
        _ColorFormat ("NTSC Composite Emulation", Range(0.0, 1.0)) = 1.0
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
            Name "PSXChromaBleedPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _BleedAmount;
                float _ColorFormat;
            CBUFFER_END

            // Conversão RGB para YIQ (Espaço NTSC)
            float3 RGB2YIQ(float3 c)
            {
                return float3(
                    0.299 * c.r + 0.587 * c.g + 0.114 * c.b,
                    0.596 * c.r - 0.274 * c.g - 0.322 * c.b,
                    0.211 * c.r - 0.523 * c.g + 0.312 * c.b
                );
            }

            float3 YIQ2RGB(float3 yiq)
            {
                return float3(
                    yiq.x + 0.956 * yiq.y + 0.621 * yiq.z,
                    yiq.x - 0.272 * yiq.y - 0.647 * yiq.z,
                    yiq.x - 1.106 * yiq.y + 1.703 * yiq.z
                );
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float3 cCenter = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float3 cLeft   = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(_BleedAmount, 0)).rgb;
                float3 cRight  = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(_BleedAmount, 0)).rgb;

                float3 yiqC = RGB2YIQ(cCenter);
                float3 yiqL = RGB2YIQ(cLeft);
                float3 yiqR = RGB2YIQ(cRight);

                // Borra apenas o sinal de cor (I e Q) mantendo a luminancia (Y) afiada
                // Peso triangular (L:1 C:2 R:1) — mais fiel ao sinal NTSC
                float3 blendedYIQ = float3(
                    yiqC.x, 
                    (yiqL.y + yiqC.y * 2.0 + yiqR.y) * 0.25, 
                    (yiqL.z + yiqC.z * 2.0 + yiqR.z) * 0.25
                );

                float3 finalColor = lerp(cCenter, YIQ2RGB(blendedYIQ), _ColorFormat);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
