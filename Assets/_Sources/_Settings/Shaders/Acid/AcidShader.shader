Shader "HDRP/DitheredGradient3D_Lit"
{
    Properties
    {
        _MainTex        ("Main Texture", 2D) = "white" {}
        
        _Color1         ("Color 1", Color) = (0,0,0,1)
        _Color2         ("Color 2", Color) = (1,0,0,1)
        _Color3         ("Color 3", Color) = (1,0.5,0,1)
        _Color4         ("Color 4", Color) = (1,1,1,1)
        
        _GradientAngle  ("Gradient Angle", Range(0,360)) = 90
        _GradientSteps  ("Color Bands",   Range(2,8))    = 4
        
        _PixelSize      ("Pixel Size",    Range(1,64))   = 8
        _DitherScale    ("Dither Scale",  Float)         = 2
        _DitherStrength ("Dither Mix",    Range(0,1))    = 1
        
        _GradientScale  ("Gradient Scale", Float)        = 1
        _GlowIntensity  ("Edge Glow",     Range(0,2))    = 0
        
        [IntRange] _ColorCount ("Color Count", Range(2,4)) = 4
        [KeywordEnum(Gradient, Texture)] _ColorMode ("Color Mode", Float) = 0
        [KeywordEnum(None, Stripes, Checkerboard, Dots, Noise)] _Pattern ("Pattern", Float) = 0
        _PatternScale   ("Pattern Scale", Float) = 1
        
        _Smoothness     ("Smoothness", Range(0,1)) = 0.1
        _Metallic       ("Metallic",   Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            int _ObjectId;
            int _PassValue;

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float3 posWS = mul(UNITY_MATRIX_M, float4(input.positionOS, 1)).xyz;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                return float4(_ObjectId, _PassValue, 1.0, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ScenePickingPass"
            Tags { "LightMode" = "Picking" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            float4 _SelectionID;

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float3 posWS = mul(UNITY_MATRIX_M, float4(input.positionOS, 1)).xyz;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                return _SelectionID;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _COLORMODE_GRADIENT _COLORMODE_TEXTURE
            #pragma shader_feature_local _PATTERN_NONE _PATTERN_STRIPES _PATTERN_CHECKERBOARD _PATTERN_DOTS _PATTERN_NOISE

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color1;
                float4 _Color2;
                float4 _Color3;
                float4 _Color4;
                float  _GradientAngle;
                float  _GradientSteps;
                float  _PixelSize;
                float  _DitherScale;
                float  _DitherStrength;
                float  _GradientScale;
                float  _GlowIntensity;
                float  _ColorCount;
                float  _PatternScale;
                float  _Smoothness;
                float  _Metallic;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float2 uv         : TEXCOORD3;
            };

            static const float4x4 ditherMatrix = float4x4(
                0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
                3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
            );

            float GetDitherValue(float2 uv)
            {
                float2 p = uv * _DitherScale;
                int x = (int)floor(p.x) & 3;
                int y = (int)floor(p.y) & 3;
                return ditherMatrix[y][x];
            }

            float GetPattern(float2 uv)
            {
                float2 p = uv * _PatternScale;
                
                #ifdef _PATTERN_STRIPES
                    return frac(p.x) > 0.5 ? 1.0 : 0.0;
                #elif _PATTERN_CHECKERBOARD
                    return (frac(p.x) > 0.5) != (frac(p.y) > 0.5) ? 1.0 : 0.0;
                #elif _PATTERN_DOTS
                    float2 cell = frac(p);
                    float dist = length(cell - 0.5);
                    return dist < 0.25 ? 1.0 : 0.0;
                #elif _PATTERN_NOISE
                    return frac(sin(dot(floor(p), float2(12.9898, 78.233))) * 43758.5453);
                #else
                    return 0.5;
                #endif
            }

            Varyings Vert (Attributes input)
            {
                Varyings o;
                o.positionOS = input.positionOS;
                float3 posWS = mul(UNITY_MATRIX_M, float4(input.positionOS, 1)).xyz;
                o.positionWS = posWS;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                o.normalWS   = normalize(mul((float3x3)UNITY_MATRIX_M, input.normalOS));
                o.uv         = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            float4 Frag (Varyings i) : SV_Target
            {
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;

                float lightLevel = 0.0;
                float3 texColorMix = float3(1,1,1);

                #ifdef _COLORMODE_GRADIENT
                    float3 pos = i.positionOS;
                    float angleRad = radians(_GradientAngle);
                    float2 dir = float2(cos(angleRad), sin(angleRad));
                    float g = dot(pos.xy * _GradientScale, dir);
                    g = saturate(g * 0.5 + 0.5);
                    float patternVal = GetPattern(pixelUV);
                    lightLevel = lerp(0.0, patternVal, g);
                #elif _COLORMODE_TEXTURE
                    float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelUV);
                    lightLevel = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                    texColorMix = texColor.rgb;
                #endif

                float steps = max(2.0, _GradientSteps);
                float bandIndex = floor(lightLevel * steps);
                float localGrad = frac(lightLevel * steps);
                
                float ditherVal = GetDitherValue(pixelUV);
                
                uint colorCount = uint(max(2.0, _ColorCount));
                uint idx1 = uint(bandIndex) % colorCount;
                uint idx2 = (idx1 + 1u) % colorCount;
                
                float3 colors[4] = { _Color1.rgb, _Color2.rgb, _Color3.rgb, _Color4.rgb };
                float3 color1 = colors[idx1];
                float3 color2 = colors[idx2];
                
                float threshold = localGrad * _DitherStrength;
                float3 finalColor = (ditherVal > threshold) ? color1 : color2;
                
                #ifdef _COLORMODE_TEXTURE
                    finalColor = lerp(finalColor, finalColor * texColorMix, 0.5);
                #endif
                
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings o;
                float3 posWS = mul(UNITY_MATRIX_M, float4(input.positionOS, 1)).xyz;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                return o;
            }

            half4 ShadowFrag(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings o;
                float3 posWS = mul(UNITY_MATRIX_M, float4(input.positionOS, 1)).xyz;
                o.positionCS = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                return o;
            }

            half4 DepthFrag(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
