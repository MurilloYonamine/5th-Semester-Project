Shader "PSX/VHS"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _VhsNoise ("VHS Grain Noise", Range(0.0, 1.0)) = 0.15
        _ChromaticAberration ("Color Separation", Range(0.0, 0.05)) = 0.008
        _TrackingNoise ("Bottom Tracking Line Distortion", Range(0.0, 1.0)) = 0.25
        _TapeSpeed ("Wobble Speed", Range(0.0, 10.0)) = 2.0
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
            Name "PSXVHSPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _VhsNoise;
                float _ChromaticAberration;
                float _TrackingNoise;
                float _TapeSpeed;
            CBUFFER_END

            float Hash12(float2 p)
            {
                p = frac(p * float2(5.3983, 5.4427));
                p += dot(p.yx, p.xy + float2(21.5351, 14.3137));
                return frac(p.x * p.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float time = _Time.y * _TapeSpeed;

                // Distortion de fita analógica (Wobble horizontal)
                float wave = sin(uv.y * 30.0 + time) * 0.002 + sin(uv.y * 80.0 - time * 2.0) * 0.001;

                // Ruído de tracking de borda inferior (típico de cabeçote de VHS nas linhas inferiores)
                float trackingArea = step(uv.y, 0.12);
                float trackingNoise = Hash12(float2(uv.x * 100.0, time)) * trackingArea * _TrackingNoise;
                float trackingShift = (Hash12(float2(floor(uv.y * 50.0), time)) - 0.5) * 0.04 * trackingArea;

                float2 finalUV = uv + float2(wave + trackingShift, 0.0);

                // Desvio Cromático VHS (Aberração Cromática R/G/B)
                half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV + float2(_ChromaticAberration, 0.0)).r;
                half g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV).g;
                half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV - float2(_ChromaticAberration, 0.0)).b;
                half a = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, finalUV).a;

                // Granulação estática de fita magnética
                float grain = (Hash12(finalUV + time) - 0.5) * _VhsNoise;
                float3 finalColor = float3(r, g, b) + grain + trackingNoise;

                return half4(finalColor, a);
            }
            ENDHLSL
        }
    }
}
