Shader "Hidden/Effects/MentalityDistortion"
{
    Properties
    {
        _InnerRadius ("Undistorted Ellipse Radius (X/Y)", Vector) = (0.25, 0.30, 0, 0)
        _MaxOffset ("Maximum UV Offset", Range(0, 0.05)) = 0.012
        _WaveFrequency ("Wave Frequency", Range(1, 40)) = 14
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.2
        _RedInitial ("Red Edge Initial Strength", Range(0, 1)) = 0.38
        _RedMaximum ("Red Edge Maximum Strength", Range(0, 1)) = 0.52
    }

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
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _InnerRadius;
            float _MaxOffset;
            float _WaveFrequency;
            float _WaveSpeed;
            float _MentalityStrength;
            float _RedProgress;
            float _RedInitial;
            float _RedMaximum;
            float _WaveTime;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 centered = uv - 0.5;
                float distanceToCenter = length(centered);
                float2 direction = centered / max(distanceToCenter, 0.00001);
                float2 radii = max(_InnerRadius.xy, 0.00001);
                float innerDistance = rsqrt(dot(direction / radii, direction / radii));
                float edgeDistance = min(0.5 / max(abs(direction.x), 0.00001),
                                         0.5 / max(abs(direction.y), 0.00001));
                float edgeWeight = saturate((distanceToCenter - innerDistance) /
                                            max(edgeDistance - innerDistance, 0.00001));

                float time = _WaveTime * _WaveSpeed;
                float2 wave;
                wave.x = sin(uv.y * _WaveFrequency + time) * 0.55
                       + sin((uv.x + uv.y * 0.67) * _WaveFrequency * 1.73 - time * 1.31) * 0.30
                       + sin(uv.y * _WaveFrequency * 2.41 + time * 0.71) * 0.15;
                wave.y = sin(uv.x * _WaveFrequency * 0.83 - time * 0.87) * 0.55
                       + sin((uv.y - uv.x * 0.43) * _WaveFrequency * 1.91 + time * 1.19) * 0.30
                       + sin(uv.x * _WaveFrequency * 2.27 - time * 0.53) * 0.15;

                float2 distortedUV = saturate(uv + wave * (_MaxOffset * _MentalityStrength * edgeWeight));
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, distortedUV);
                float redStrength = _RedProgress < 0.0 ? 0.0 : lerp(_RedInitial, _RedMaximum, _RedProgress);
                float redWeight = redStrength * smoothstep(0.0, 0.9, edgeWeight);
                color.rgb = lerp(color.rgb, color.rgb * half3(1.0, 0.12, 0.12) + half3(0.15, 0.0, 0.0), redWeight);
                return color;
            }
            ENDHLSL
        }
    }
}
