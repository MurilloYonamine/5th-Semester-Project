Shader "PSX/ScanLines_Soft"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _Count ("Line Count", Range(50.0, 1200.0)) = 240.0
        _Intensity ("Scanline Intensity", Range(0.0, 1.0)) = 0.35
        _Speed ("Drift Speed", Range(-5.0, 5.0)) = 0.1
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
            Name "PSXScanLinesSoftPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Count;
                float _Intensity;
                float _Speed;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                
                // Linhas de varredura suaves estilo tubo de imagem analógico
                float scanline = sin((input.texcoord.y + _Time.y * _Speed) * _Count * 3.14159265) * 0.5 + 0.5;
                float shadow = lerp(1.0 - _Intensity, 1.0, scanline);

                return half4(col.rgb * shadow, col.a);
            }
            ENDHLSL
        }
    }
}
