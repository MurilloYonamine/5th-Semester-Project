Shader "PSX/Pixelate"
{
    Properties
    {
        [HideInInspector] _BlitTexture ("Source Texture", 2D) = "white" {}
        _PixelResolutionX ("Target Resolution Width", Float) = 320.0
        _PixelResolutionY ("Target Resolution Height", Float) = 240.0
        _UseAspectCorrection ("Maintain Aspect Ratio", Float) = 1.0
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
            Name "PSXPixelatePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _PixelResolutionX;
                float _PixelResolutionY;
                float _UseAspectCorrection;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                float2 targetRes = float2(_PixelResolutionX, _PixelResolutionY);
                
                if (_UseAspectCorrection > 0.5)
                {
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    targetRes.x = targetRes.y * aspect;
                }

                // Cálculo da grade de pixelização retro
                float2 pixelatedUV = floor(input.texcoord * targetRes) / targetRes + (0.5 / targetRes);

                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, pixelatedUV);
            }
            ENDHLSL
        }
    }
}
