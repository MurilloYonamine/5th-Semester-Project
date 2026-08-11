Shader "PSX/SMPTE"
{
    Properties
    {
        _NoiseStrength ("Analog Noise", Range(0.0, 1.0)) = 0.05
        _ScanlineStrength ("CRT Scanlines", Range(0.0, 1.0)) = 0.1
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

        Pass
        {
            Name "PSXSMPTEPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _NoiseStrength;
                float _ScanlineStrength;
            CBUFFER_END

            float Random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float3 col = float3(0, 0, 0);

                // 1. Barras Superiores (75% de intensidade) - 67% da altura total
                if (uv.y > 0.33)
                {
                    int barIndex = (int)(uv.x * 7.0);
                    switch (barIndex)
                    {
                        case 0: col = float3(0.75, 0.75, 0.75); break; // Grey/White
                        case 1: col = float3(0.75, 0.75, 0.00); break; // Yellow
                        case 2: col = float3(0.00, 0.75, 0.75); break; // Cyan
                        case 3: col = float3(0.00, 0.75, 0.00); break; // Green
                        case 4: col = float3(0.75, 0.00, 0.75); break; // Magenta
                        case 5: col = float3(0.75, 0.00, 0.00); break; // Red
                        case 6: col = float3(0.00, 0.00, 0.75); break; // Blue
                    }
                }
                // 2. Barras Médias (Reverse Blue) - entre 25% e 33% da altura
                else if (uv.y > 0.25)
                {
                    int barIndex = (int)(uv.x * 7.0);
                    switch (barIndex)
                    {
                        case 0: col = float3(0.00, 0.00, 0.75); break; // Blue
                        case 1: col = float3(0.00, 0.00, 0.00); break; // Black
                        case 2: col = float3(0.75, 0.00, 0.75); break; // Magenta
                        case 3: col = float3(0.00, 0.00, 0.00); break; // Black
                        case 4: col = float3(0.00, 0.75, 0.75); break; // Cyan
                        case 5: col = float3(0.00, 0.00, 0.00); break; // Black
                        case 6: col = float3(0.75, 0.75, 0.75); break; // White
                    }
                }
                // 3. Barras Inferiores (PLUGE / NTSC Calibration)
                else
                {
                    if (uv.x < 0.18)      col = float3(0.0, 0.15, 0.3);  // -I
                    else if (uv.x < 0.36) col = float3(1.0, 1.0, 1.0);    // 100% White
                    else if (uv.x < 0.54) col = float3(0.25, 0.0, 0.4);   // +Q
                    else if (uv.x < 0.71) col = float3(0.0, 0.0, 0.0);    // Black
                    else                  col = float3(0.05, 0.05, 0.05);// PLUGE
                }

                // Ruído e scanlines de TV analógica
                float noise = (Random(uv + _Time.y) - 0.5) * _NoiseStrength;
                float scanline = sin(uv.y * 480.0 * 3.14159) * 0.5 + 0.5;
                col = lerp(col, col * scanline, _ScanlineStrength) + noise;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
