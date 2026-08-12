Shader "PSX/15bit_Color"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _BitDepth ("Bit Depth Per Channel", Range(3.0, 8.0)) = 5.0
        [Toggle(_DITHER_BANDING)] _DitherBanding ("Dither to Hide Banding", Float) = 0.0
        _DitherStrength ("Dither Strength", Range(0.0, 1.0)) = 0.5
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
            Name "PSX15bitColorPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _DITHER_BANDING

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _BitDepth;
                float _DitherStrength;
            CBUFFER_END

            // Matriz Bayer 4x4 para dithering de banding de cor
            static const float4x4 BAYER_4X4 = float4x4(
                 0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                 3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
                15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                // Niveis de cor por canal: 2^BitDepth (ex: 5 bits = 32 niveis = PS1 autentico)
                float levels = pow(2.0, _BitDepth) - 1.0;

                #if defined(_DITHER_BANDING)
                    // Aplica dithering Bayer ANTES de quantizar para suavizar o banding
                    uint2 px = (uint2)input.positionCS.xy;
                    float dither = BAYER_4X4[px.x % 4][px.y % 4] - 0.5;
                    col.rgb += (dither / levels) * _DitherStrength;
                #endif

                // Quantizacao para N bits por canal (PS1 = 5 bits = 32 niveis por canal R/G/B)
                col.rgb = floor(col.rgb * levels + 0.5) / levels;

                return col;
            }
            ENDHLSL
        }
    }
}
