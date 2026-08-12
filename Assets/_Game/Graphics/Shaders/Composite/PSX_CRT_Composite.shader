Shader "PSX/CRT_Composite"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}

        [Header(Pixelation)]
        [Toggle(_PIXELATE_ON)] _EnablePixelate ("Enable Pixelation", Float) = 1.0
        _PixelResolutionX ("Resolution Width", Float) = 320.0
        _PixelResolutionY ("Resolution Height", Float) = 240.0

        [Header(CRT Barrel Distortion)]
        [Toggle(_BARREL_ON)] _EnableBarrel ("Enable Barrel Distortion", Float) = 1.0
        _BarrelStrength ("Distortion Strength", Range(-1.0, 1.0)) = 0.12
        _BarrelTightness ("Tightness", Range(0.1, 10.0)) = 3.0
        _BarrelZoom ("Zoom", Range(0.5, 2.0)) = 0.98
        _Vignette ("Corner Vignette", Range(0.0, 1.0)) = 0.4

        [Header(Bayer Dithering)]
        [Toggle(_DITHER_ON)] _EnableDither ("Enable Dithering", Float) = 1.0
        _DitherSpread ("Quantization Levels", Range(2.0, 64.0)) = 16.0
        _DitherStrength ("Dither Intensity", Range(0.0, 1.0)) = 0.7

        [Header(Chroma Bleed Analogo RCA)]
        [Toggle(_CHROMA_BLEED_ON)] _EnableChromaBleed ("Enable Chroma Bleed", Float) = 1.0
        _BleedAmount ("Bleed Spread", Range(0.0, 0.02)) = 0.005

        [Header(Scanlines and Rolling Bands)]
        [Toggle(_SCANLINES_ON)] _EnableScanlines ("Enable Scanlines", Float) = 1.0
        _ScanlineCount ("Scanline Count", Range(50.0, 1200.0)) = 240.0
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 1.0)) = 0.3
        _RollingBandSpeed ("Band Speed", Range(-10.0, 10.0)) = 1.0
        _RollingBandIntensity ("Band Intensity", Range(0.0, 1.0)) = 0.1

        [Header(Glitch and VHS Tape Noise)]
        [Toggle(_GLITCH_ON)] _EnableGlitch ("Enable VHS / Glitch", Float) = 1.0
        _GlitchAmount ("Jitter Amount", Range(0.0, 0.1)) = 0.01
        _VhsGrain ("Tape Grain Noise", Range(0.0, 1.0)) = 0.1
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
            Name "PSXCRTCompositePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma shader_feature_local _PIXELATE_ON
            #pragma shader_feature_local _BARREL_ON
            #pragma shader_feature_local _DITHER_ON
            #pragma shader_feature_local _CHROMA_BLEED_ON
            #pragma shader_feature_local _SCANLINES_ON
            #pragma shader_feature_local _GLITCH_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _EnablePixelate;
                float _PixelResolutionX;
                float _PixelResolutionY;

                float _EnableBarrel;
                float _BarrelStrength;
                float _BarrelTightness;
                float _BarrelZoom;
                float _Vignette;

                float _EnableDither;
                float _DitherSpread;
                float _DitherStrength;

                float _EnableChromaBleed;
                float _BleedAmount;

                float _EnableScanlines;
                float _ScanlineCount;
                float _ScanlineIntensity;
                float _RollingBandSpeed;
                float _RollingBandIntensity;

                float _EnableGlitch;
                float _GlitchAmount;
                float _VhsGrain;
            CBUFFER_END

            static const float4x4 BAYER_4X4 = float4x4(
                0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
               12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
               15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            float Hash12(float2 p)
            {
                p = frac(p * float2(5.3983, 5.4427));
                p += dot(p.yx, p.xy + float2(21.5351, 14.3137));
                return frac(p.x * p.y);
            }

            // Conversão RGB para YIQ (Espaço de cor NTSC)
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
                // rawUV preserva as coordenadas originais de tela para dithering e scanlines
                // Isso evita que esses efeitos sejam distorcidos pelo barrel ou pela pixelacao
                float2 rawUV = input.texcoord;
                float2 uv = rawUV;

                // 1. Barrel Distortion CRT (com correcao de aspect ratio para curvatura circular)
                float edgeMask = 1.0;
                #if defined(_BARREL_ON)
                if (_EnableBarrel > 0.5)
                {
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    float2 centeredUV = uv - 0.5;
                    // Aspect ratio so entra no calculo do raio, nao na distorcao do UV de sample
                    float2 aspectUV = centeredUV * float2(aspect, 1.0);
                    float r2 = dot(aspectUV, aspectUV);
                    float distortion = 1.0 + _BarrelStrength * pow(abs(r2), _BarrelTightness * 0.5);
                    uv = (centeredUV * distortion) * _BarrelZoom + 0.5;

                    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    {
                        return half4(0, 0, 0, 1);
                    }

                    float edgeX = smoothstep(0.0, 0.04, uv.x) * smoothstep(1.0, 0.96, uv.x);
                    float edgeY = smoothstep(0.0, 0.04, uv.y) * smoothstep(1.0, 0.96, uv.y);
                    edgeMask = lerp(1.0, edgeX * edgeY, _Vignette);
                }
                #endif

                // 2. Glitch / VHS Jitter
                #if defined(_GLITCH_ON)
                if (_EnableGlitch > 0.5)
                {
                    float time = _Time.y * 12.0;
                    float lineNoise = Hash12(float2(floor(uv.y * 100.0), floor(time)));
                    if (lineNoise > 0.88)
                    {
                        uv.x += (Hash12(float2(time, uv.y)) - 0.5) * _GlitchAmount;
                    }
                }
                #endif

                // 3. Pixelation
                #if defined(_PIXELATE_ON)
                if (_EnablePixelate > 0.5)
                {
                    float2 targetRes = float2(_PixelResolutionX, _PixelResolutionY);
                    uv = floor(uv * targetRes) / targetRes + (0.5 / targetRes);
                }
                #endif

                // 4. Chroma Bleed (Sangramento Analógico RCA)
                half3 color;
                #if defined(_CHROMA_BLEED_ON)
                if (_EnableChromaBleed > 0.5)
                {
                    float3 centerCol = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                    float3 leftCol   = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(_BleedAmount, 0)).rgb;
                    float3 rightCol  = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(_BleedAmount, 0)).rgb;

                    float3 yiqC = RGB2YIQ(centerCol);
                    float3 yiqL = RGB2YIQ(leftCol);
                    float3 yiqR = RGB2YIQ(rightCol);

                    // Borra o sinal I e Q (Crominancia) horizontalmente mantendo o Y (Luminancia) nitido
                    // Peso triangular (L:1 C:2 R:1) — consistente com PSX_ChromaBleed.shader
                    float3 finalYIQ = float3(yiqC.x, (yiqL.y + yiqC.y * 2.0 + yiqR.y) * 0.25, (yiqL.z + yiqC.z * 2.0 + yiqR.z) * 0.25);
                    color = YIQ2RGB(finalYIQ);
                }
                else
                {
                    color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                }
                #else
                color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                #endif

                // 5. Bayer Dithering
                // IMPORTANTE: usa rawUV (nao o uv pixelado/distorcido) para alinhar a matriz Bayer
                // ao grid de pixels fisicos da tela — evita o padrao de Moire circular
                #if defined(_DITHER_ON)
                if (_EnableDither > 0.5)
                {
                    uint2 pixelPos = (uint2)(rawUV * _ScreenParams.xy);
                    float dither = BAYER_4X4[pixelPos.x % 4][pixelPos.y % 4] - 0.5;
                    float3 dithered = color + (dither * _DitherStrength / _DitherSpread);
                    color = floor(dithered * _DitherSpread) / _DitherSpread;
                }
                #endif

                // 6. Scanlines & Rolling Bands
                // usa rawUV.y para linhas horizontais retas de TV CRT
                // (uv ja foi distorcido pelo barrel — usalo aqui curvaria as linhas)
                #if defined(_SCANLINES_ON)
                if (_EnableScanlines > 0.5)
                {
                    float scanline = sin(rawUV.y * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                    color *= lerp(1.0 - _ScanlineIntensity, 1.0, scanline);

                    float roll = sin(rawUV.y * 10.0 - _Time.y * _RollingBandSpeed) * 0.5 + 0.5;
                    color = lerp(color, color * 0.7, roll * _RollingBandIntensity);
                }
                #endif

                // 7. VHS Tape Noise
                #if defined(_GLITCH_ON)
                if (_EnableGlitch > 0.5 && _VhsGrain > 0.0)
                {
                    float grain = (Hash12(uv + _Time.y) - 0.5) * _VhsGrain;
                    color += grain;
                }
                #endif

                return half4(color * edgeMask, 1.0);
            }
            ENDHLSL
        }
    }
}
