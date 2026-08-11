Shader "PSX/RollingBands"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _Speed ("Band Speed", Range(-10.0, 10.0)) = 1.5
        _BandFrequency ("Band Frequency", Range(1.0, 50.0)) = 8.0
        _Intensity ("Band Intensity", Range(0.0, 1.0)) = 0.15
        _BandColor ("Band Color Tint", Color) = (0.0, 0.0, 0.0, 1.0)
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
            Name "PSXRollingBandsPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Speed;
                float _BandFrequency;
                float _Intensity;
                float4 _BandColor;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                
                // Calculo das faixas de rolagem da TV analógica
                float band = sin(input.texcoord.y * _BandFrequency - _Time.y * _Speed) * 0.5 + 0.5;
                band = pow(band, 3.0); // Deixa as faixas mais suaves no centro e marcadas nas bordas

                float3 blendedColor = lerp(col.rgb, _BandColor.rgb, band * _Intensity);
                return half4(blendedColor, col.a);
            }
            ENDHLSL
        }
    }
}
