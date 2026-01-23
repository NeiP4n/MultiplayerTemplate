Shader "HDRP/OutlineUnlit_Pencil"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.015
        _NoiseScale ("Noise Scale", Float) = 40
        _Breakup ("Line Breakup", Range(0,1)) = 0.45
        _Alpha ("Alpha", Range(0,1)) = 1
        _EdgeSoftness ("Edge Softness", Range(0,1)) = 0.3
        _BlurStrength ("Blur Strength", Range(0,1)) = 0.5
        _AnimationSpeed ("Animation Speed", Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "Outline"
            Cull Off
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
                float3 viewDir    : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
                float _NoiseScale;
                float _Breakup;
                float _Alpha;
                float _EdgeSoftness;
                float _BlurStrength;
                float _AnimationSpeed;
            CBUFFER_END

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 3; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            Varyings vert (Attributes input)
            {
                Varyings output;

                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                float timeOffset = _Time.y * _AnimationSpeed * 0.3;
                float noiseVariation = noise(positionWS.xz * 5.0 + timeOffset) * 0.3 + 0.85;
                float width = _OutlineWidth * noiseVariation;
                
                positionWS += normalWS * width;

                output.positionCS = TransformWorldToHClip(positionWS);
                output.worldPos = positionWS;
                output.viewDir = normalize(GetCameraRelativePositionWS(positionWS));

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float timeScroll = _Time.y * _AnimationSpeed;
                float2 noiseUV = input.worldPos.xz * _NoiseScale;
                
                float noise1 = fbm(noiseUV + float2(timeScroll, timeScroll * 0.5));
                float noise2 = fbm(noiseUV * 1.7 + float2(-timeScroll * 0.7, timeScroll * 0.9)) * 0.5;
                float combinedNoise = (noise1 + noise2) * 0.66;

                float breakupThreshold = _Breakup + _EdgeSoftness * (combinedNoise - 0.5);
                float edgeGradient = smoothstep(
                    breakupThreshold - _BlurStrength * 0.15, 
                    breakupThreshold + _BlurStrength * 0.15, 
                    combinedNoise
                );

                if (edgeGradient < 0.05)
                    discard;

                float pressureVariation = lerp(0.3, 1.0, combinedNoise);
                float edgeFade = pow(edgeGradient, 0.8);
                float alpha = _Alpha * pressureVariation * edgeFade;

                float3 finalColor = lerp(
                    _OutlineColor.rgb * 1.2, 
                    _OutlineColor.rgb, 
                    edgeGradient
                );

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
