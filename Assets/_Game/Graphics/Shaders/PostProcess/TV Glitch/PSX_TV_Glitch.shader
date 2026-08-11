Shader "PSX/TV_Glitch"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _GlitchFrequency ("Glitch Speed/Frequency", Range(0.0, 50.0)) = 15.0
        _GlitchAmount ("Horizontal Jitter", Range(0.0, 0.1)) = 0.02
        _ColorSplit ("RGB Chromatic Split", Range(0.0, 0.05)) = 0.008
        _NoiseScale ("Noise Density", Float) = 120.0
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
            Name "PSXTVGlitchPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _GlitchFrequency;
                float _GlitchAmount;
                float _ColorSplit;
                float _NoiseScale;
            CBUFFER_END

            float PseudoRandom(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float time = _Time.y * _GlitchFrequency;

                // Ruído de linha para jitter horizontal
                float lineNoise = PseudoRandom(float2(floor(uv.y * _NoiseScale), floor(time)));
                float glitchMask = step(0.85, lineNoise); // Ativa o glitch em blocos aleatórios
                float offset = (PseudoRandom(float2(time, uv.y)) - 0.5) * _GlitchAmount * glitchMask;

                float2 glitchUV = uv + float2(offset, 0.0);

                // Separação de canais RGB (Chromatic Aberration)
                half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, glitchUV + float2(_ColorSplit, 0.0)).r;
                half g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, glitchUV).g;
                half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, glitchUV - float2(_ColorSplit, 0.0)).b;
                half a = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, glitchUV).a;

                // Estática de sinal analógico no bloco do glitch
                float staticNoise = (PseudoRandom(glitchUV * time) - 0.5) * 0.1 * glitchMask;

                return half4(r + staticNoise, g + staticNoise, b + staticNoise, a);
            }
            ENDHLSL
        }
    }
}
