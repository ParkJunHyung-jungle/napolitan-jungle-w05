Shader "Hidden/New Outline"
{
    Properties
    {
        [MainColor] _Outline_Color ("Outline Color", Color) = (1, 0.92, 0, 1)
        _Outline_Width ("Outline Width (Pixels)", Float) = 2
        [HideInInspector] _OutlineMask_TexelSize ("Outline Mask Texel Size", Vector) = (0.001, 0.001, 1000, 1000)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "GeometryMask"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZTest LEqual
            ZWrite Off
            Blend One Zero
            ColorMask R

            HLSLPROGRAM
            #pragma vertex MaskVertex
            #pragma fragment MaskFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct MaskAttributes
            {
                float4 positionOS : POSITION;
            };

            struct MaskVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            MaskVaryings MaskVertex(MaskAttributes input)
            {
                MaskVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 MaskFragment(MaskVaryings input) : SV_Target
            {
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ScreenSpaceComposite"
            Cull Off
            ZTest Always
            ZWrite Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_OutlineMask);
            SAMPLER(sampler_OutlineMask);

            CBUFFER_START(UnityPerMaterial)
                half4 _Outline_Color;
                float _Outline_Width;
                float4 _OutlineMask_TexelSize;
            CBUFFER_END

            half SampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_OutlineMask, sampler_OutlineMask, uv).r;
            }

            half DilatedMask(float2 uv)
            {
                float2 texel = _OutlineMask_TexelSize.xy * max(_Outline_Width, 0.0);
                half result = SampleMask(uv);
                result = max(result, SampleMask(uv + float2(texel.x, 0)));
                result = max(result, SampleMask(uv + float2(-texel.x, 0)));
                result = max(result, SampleMask(uv + float2(0, texel.y)));
                result = max(result, SampleMask(uv + float2(0, -texel.y)));
                result = max(result, SampleMask(uv + texel));
                result = max(result, SampleMask(uv + float2(-texel.x, texel.y)));
                result = max(result, SampleMask(uv + float2(texel.x, -texel.y)));
                result = max(result, SampleMask(uv - texel));
                return result;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half4 cameraColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                half originalMask = SampleMask(input.texcoord);
                half outlineMask = saturate(DilatedMask(input.texcoord) - originalMask);
                return lerp(cameraColor, _Outline_Color, outlineMask * _Outline_Color.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "CameraColorCopy"
            Cull Off
            ZTest Always
            ZWrite Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment CopyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 CopyFragment(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
            }
            ENDHLSL
        }
    }
}
