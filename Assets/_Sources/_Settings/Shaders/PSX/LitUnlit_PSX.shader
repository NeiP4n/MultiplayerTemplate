Shader "HDRP/LitUnlit_PSX_Fixed"
{
    Properties
    {
        [Toggle(UNLIT_MODE)] _UnlitMode ("Unlit", Float) = 0
        _MainTex ("Base", 2D) = "white" {}
        _SnowTex ("Snow", 2D) = "white" {}
        _Color ("Tint", Color) = (0.85,0.92,1,1)
        _SnowAmount ("Snow", Range(0,1)) = 0.5
        _PixelSize ("World Pixel", Float) = 0.1
        _ColorVariation ("Color Var", Range(0,0.3)) = 0.08
        _ResolutionX ("Res X", Float) = 320
        _ResolutionY ("Res Y", Float) = 280
        _DitherStrength ("Dither", Range(0,1)) = 0.5
        _UnlitBrightness ("Unlit Bright", Range(0,2)) = 0.4
        _LightDir ("Fake Light Dir", Vector) = (0.3,0.8,0.4,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="ForwardOnly" }
            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local UNLIT_MODE

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float3 posOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float snow : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SnowTex); SAMPLER(sampler_SnowTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _SnowAmount;
                float _PixelSize;
                float _ColorVariation;
                float _ResolutionX;
                float _ResolutionY;
                float _DitherStrength;
                float _UnlitBrightness;
                float4 _LightDir;
            CBUFFER_END

            static const float4x4 ditherTable = float4x4(
                0,8,2,10,
                12,4,14,6,
                3,11,1,9,
                15,7,13,5
            ) / 16.0;

            float hash(float2 p)
            {
                return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);
            }

            float4 SampleTri(TEXTURE2D_PARAM(tex,smp), float3 ws, float3 n)
            {
                float3 w = pow(abs(n),6);
                w /= dot(w,1);
                float2 ux = floor(ws.zy/_PixelSize)*_PixelSize;
                float2 uy = floor(ws.xz/_PixelSize)*_PixelSize;
                float2 uz = floor(ws.xy/_PixelSize)*_PixelSize;
                return
                    SAMPLE_TEXTURE2D(tex,smp,ux)*w.x +
                    SAMPLE_TEXTURE2D(tex,smp,uy)*w.y +
                    SAMPLE_TEXTURE2D(tex,smp,uz)*w.z;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);

                float3 posWS = mul(GetObjectToWorldMatrix(), float4(v.posOS,1)).xyz;
                float3 normalWS = normalize(mul((float3x3)GetObjectToWorldMatrix(), v.normalOS));

                float4 posCS = TransformWorldToHClip(posWS);
                float2 ndc = posCS.xy / max(posCS.w, 1e-5);
                ndc = floor(ndc * float2(_ResolutionX,_ResolutionY)) / float2(_ResolutionX,_ResolutionY);
                posCS.xy = ndc * posCS.w;

                o.posCS = posCS;
                o.posWS = posWS;
                o.normalWS = normalWS;
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.snow = saturate(dot(normalWS,float3(0,1,0)) * _SnowAmount);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                half4 baseCol = SampleTri(TEXTURE2D_ARGS(_MainTex,sampler_MainTex), i.posWS, i.normalWS);
                half4 snowCol = SampleTri(TEXTURE2D_ARGS(_SnowTex,sampler_SnowTex), i.posWS, i.normalWS);
                half4 col = lerp(baseCol, snowCol, i.snow) * _Color;

                float rnd = hash(floor(i.posWS.xz/_PixelSize));
                col.rgb *= 1 + (rnd-0.5)*_ColorVariation;

                half3 lighting = half3(_UnlitBrightness,_UnlitBrightness,_UnlitBrightness);

                #ifndef UNLIT_MODE
                    float3 ldir = normalize(_LightDir.xyz);
                    lighting += saturate(dot(i.normalWS, ldir));
                #endif

                col.rgb *= lighting;

                int2 p = int2(floor(fmod(i.posCS.xy,4)));
                float d = ditherTable[p.x][p.y];
                col.rgb = floor(col.rgb*31 + d*_DitherStrength)/31;

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes { float3 posOS:POSITION; };
            struct Varyings { float4 posCS:SV_POSITION; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 ws = mul(GetObjectToWorldMatrix(), float4(v.posOS,1)).xyz;
                o.posCS = TransformWorldToHClip(ws);
                return o;
            }

            half4 frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
