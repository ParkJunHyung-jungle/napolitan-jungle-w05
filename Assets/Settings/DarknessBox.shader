Shader "Hidden/Effects/DarknessBox"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float3 _BoxMin;
            float3 _BoxMax;
            float4x4 _WorldToBox;
            float _Density;
            float4 _DarknessColor;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float rawDepth = SampleSceneDepth(uv);
                #if UNITY_REVERSED_Z
                    float depth = rawDepth;
                    float nearDepth = 1.0;
                    bool sky = rawDepth == 0.0;
                #else
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, rawDepth);
                    float nearDepth = UNITY_NEAR_CLIP_VALUE;
                    bool sky = rawDepth == 1.0;
                #endif

                float3 rayPoint = ComputeWorldSpacePosition(uv, 0.5, UNITY_MATRIX_I_VP);
                float3 nearPoint = ComputeWorldSpacePosition(uv, nearDepth, UNITY_MATRIX_I_VP);
                float3 origin = lerp(_WorldSpaceCameraPos, nearPoint, unity_OrthoParams.w);
                float3 direction = normalize(rayPoint - origin);
                float surfaceDistance = _ProjectionParams.z;
                if (!sky)
                    surfaceDistance = distance(origin, ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP));
                float3 localOrigin = mul(_WorldToBox, float4(origin, 1.0)).xyz;
                float3 localDirection = mul((float3x3)_WorldToBox, direction);

                float entry = 0.0;
                float exit = surfaceDistance;
                for (int axis = 0; axis < 3; axis++)
                {
                    if (localDirection[axis] == 0.0)
                    {
                        if (localOrigin[axis] < _BoxMin[axis] || localOrigin[axis] > _BoxMax[axis])
                            return color;
                        continue;
                    }

                    float first = (_BoxMin[axis] - localOrigin[axis]) / localDirection[axis];
                    float second = (_BoxMax[axis] - localOrigin[axis]) / localDirection[axis];
                    entry = max(entry, min(first, second));
                    exit = min(exit, max(first, second));
                }

                float distanceInBox = max(0.0, exit - entry);
                color.rgb = lerp(color.rgb, _DarknessColor.rgb, 1.0 - exp(-_Density * distanceInBox));
                return color;
            }
            ENDHLSL
        }
    }
}
