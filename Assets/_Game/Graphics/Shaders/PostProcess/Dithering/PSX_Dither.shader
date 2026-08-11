Shader "PSX/Dither"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _DitherSpread ("Color Quantization Levels", Range(2.0, 64.0)) = 16.0
        _DitherStrength ("Dither Strength", Range(0.0, 1.0)) = 0.8
        _PatternScale ("Pattern Scale", Range(1.0, 8.0)) = 1.0
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
            Name "PSXDitherPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _DitherSpread;
                float _DitherStrength;
                float _PatternScale;
            CBUFFER_END

            // Matriz Bayer 4x4 padrão PS1
            static const float4x4 BAYER_MATRIX = float4x4(
                0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
               12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
               15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            float GetBayerValue(uint2 pixelPos)
            {
                uint x = (pixelPos.x / (uint)_PatternScale) % 4;
                uint y = (pixelPos.y / (uint)_PatternScale) % 4;
                return BAYER_MATRIX[x][y];
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                
                uint2 pixelPos = (uint2)(input.texcoord * _ScreenParams.xy);
                float dither = GetBayerValue(pixelPos) - 0.5;

                // Aplicação da quantização de cores com dithering
                float3 ditheredColor = col.rgb + (dither * _DitherStrength / _DitherSpread);
                float3 quantizedColor = floor(ditheredColor * _DitherSpread) / _DitherSpread;

                return half4(quantizedColor, col.a);
            }
            ENDHLSL
        }
    }
}
