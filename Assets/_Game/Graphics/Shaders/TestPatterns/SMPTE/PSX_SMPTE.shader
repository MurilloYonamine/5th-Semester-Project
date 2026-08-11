Shader "PSX/SMPTE"
{
    Properties
    {
        [HideInInspector] _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseStrength ("Analog Noise", Range(0.0, 1.0)) = 0.05
        _ScanlineStrength ("CRT Scanlines", Range(0.0, 1.0)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
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

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float _NoiseStrength;
                float _ScanlineStrength;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Garante que o UV esteje no intervalo [0, 1]
                float2 uv = saturate(input.uv);
                float3 col = float3(0, 0, 0);

                // 1. SECAO SUPERIOR (75% da altura total) - 7 barras de cores principais
                if (uv.y > 0.33)
                {
                    float bar = uv.x * 7.0;
                    if (bar < 1.0)      col = float3(0.75, 0.75, 0.75); // 0: Cinza 75%
                    else if (bar < 2.0) col = float3(0.75, 0.75, 0.00); // 1: Amarelo
                    else if (bar < 3.0) col = float3(0.00, 0.75, 0.75); // 2: Ciano
                    else if (bar < 4.0) col = float3(0.00, 0.75, 0.00); // 3: Verde
                    else if (bar < 5.0) col = float3(0.75, 0.00, 0.75); // 4: Magenta
                    else if (bar < 6.0) col = float3(0.75, 0.00, 0.00); // 5: Vermelho
                    else                col = float3(0.00, 0.00, 0.75); // 6: Azul
                }
                // 2. SECAO INTERMEDIARIA (8% da altura) - Barras de contraste azul invertidas
                else if (uv.y > 0.25)
                {
                    float bar = uv.x * 7.0;
                    if (bar < 1.0)      col = float3(0.00, 0.00, 0.75); // Azul
                    else if (bar < 2.0) col = float3(0.05, 0.05, 0.05); // Preto
                    else if (bar < 3.0) col = float3(0.75, 0.00, 0.75); // Magenta
                    else if (bar < 4.0) col = float3(0.05, 0.05, 0.05); // Preto
                    else if (bar < 5.0) col = float3(0.00, 0.75, 0.75); // Ciano
                    else if (bar < 6.0) col = float3(0.05, 0.05, 0.05); // Preto
                    else                col = float3(0.75, 0.75, 0.75); // Cinza 75%
                }
                // 3. SECAO INFERIOR (25% da altura) - Sinais PLUGE e calibracao NTSC
                else
                {
                    if (uv.x < 0.1428)      col = float3(0.00, 0.14, 0.28); // Azul Marinho (-I)
                    else if (uv.x < 0.2857) col = float3(1.00, 1.00, 1.00); // Branco 100%
                    else if (uv.x < 0.4285) col = float3(0.22, 0.00, 0.42); // Roxo Escuro (+Q)
                    else if (uv.x < 0.5714) col = float3(0.05, 0.05, 0.05); // Preto Referencia
                    else if (uv.x < 0.7142)
                    {
                        // Barras PLUGE de Calibracao (-4%, 0%, +4%)
                        float subBar = (uv.x - 0.5714) / 0.1428;
                        if (subBar < 0.33)      col = float3(0.02, 0.02, 0.02); // Super Black (-4%)
                        else if (subBar < 0.66) col = float3(0.05, 0.05, 0.05); // Black (0%)
                        else                    col = float3(0.09, 0.09, 0.09); // Light Black (+4%)
                    }
                    else if (uv.x < 0.8571) col = float3(0.05, 0.05, 0.05); // Preto
                    else                    col = float3(0.12, 0.12, 0.12); // Cinza Escuro
                }

                // Ruido estatico de TV analogica e linhas de varredura
                if (_ScanlineStrength > 0.0)
                {
                    float scanline = sin(uv.y * 480.0 * 3.14159) * 0.5 + 0.5;
                    col = lerp(col, col * scanline, _ScanlineStrength);
                }

                if (_NoiseStrength > 0.0)
                {
                    float noise = (Random(uv + _Time.y) - 0.5) * _NoiseStrength;
                    col += noise;
                }

                return half4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }
}
