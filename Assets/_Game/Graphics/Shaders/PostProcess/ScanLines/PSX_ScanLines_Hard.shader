Shader "PSX/ScanLines_Hard"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _Count ("Line Count", Range(50.0, 1200.0)) = 240.0
        _Intensity ("Scanline Opacity", Range(0.0, 1.0)) = 0.5
        _Hardness ("Line Hardness", Range(1.0, 30.0)) = 12.0
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
            Name "PSXScanLinesHardPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Count;
                float _Intensity;
                float _Hardness;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                
                // Linhas nitidas de varredura CRT (Hard Scanlines)
                // sin() * 0.5 + 0.5 mantem a frequencia correta (abs duplicaria)
                float lineWave = sin(input.texcoord.y * _Count * 3.14159265) * 0.5 + 0.5;
                float scanline = pow(lineWave, _Hardness);

                float3 finalColor = lerp(col.rgb * (1.0 - _Intensity), col.rgb, scanline);
                return half4(finalColor, col.a);
            }
            ENDHLSL
        }
    }
}
