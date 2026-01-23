Shader "HDRP/WinterTerrain"
{
    Properties
    {
        [Toggle(UNLIT_MODE)] _UnlitMode ("Unlit", Float) = 0
        _MainTex ("Base", 2D) = "white" {}
        _Color ("Winter Tint", Color) = (0.85, 0.92, 1.0, 1.0)
        _SnowTex ("Snow", 2D) = "white" {}
        _SnowAmount ("Snow", Range(0,1)) = 0.5
        _DitherStrength ("Dither", Range(0,1)) = 0.5
        _ResolutionX ("Width", Float) = 320
        _ResolutionY ("Height", Float) = 280
        _PixelSize ("Pixel Size", Float) = 0.1
        _ColorVariation ("Variation", Range(0,0.3)) = 0.08
        _UnlitBrightness ("Brightness", Range(0,2)) = 0.3
        _TerrainHeight ("Height Map", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" "RenderType"="HDLitShader" "Queue"="Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "Forward" }
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            #pragma multi_compile_instancing
            #pragma shader_feature_local UNLIT_MODE
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float height : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SnowTex); SAMPLER(sampler_SnowTex);
            TEXTURE2D(_TerrainHeight); SAMPLER(sampler_TerrainHeight);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST, _Color;
                float _SnowAmount, _DitherStrength, _ResolutionX, _ResolutionY;
                float _PixelSize, _ColorVariation, _UnlitBrightness;
            CBUFFER_END

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float4 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 ws, float3 n)
            {
                float3 w = pow(abs(n), 8.0);
                w /= dot(w, 1.0);
                float2 uvX = floor(ws.zy / _PixelSize) * _PixelSize;
                float2 uvY = floor(ws.xz / _PixelSize) * _PixelSize;
                float2 uvZ = floor(ws.xy / _PixelSize) * _PixelSize;
                return SAMPLE_TEXTURE2D(tex, samp, uvX) * w.x +
                       SAMPLE_TEXTURE2D(tex, samp, uvY) * w.y +
                       SAMPLE_TEXTURE2D(tex, samp, uvZ) * w.z;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 posWS = TransformObjectToWorld(v.posOS.xyz);
                o.posCS = TransformWorldToHClip(posWS);
                o.posCS.xy = floor(o.posCS.xy/o.posCS.w * float2(_ResolutionX,_ResolutionY)) / float2(_ResolutionX,_ResolutionY) * o.posCS.w;
                o.posWS = posWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.height = SAMPLE_TEXTURE2D_LOD(_TerrainHeight, sampler_TerrainHeight, v.uv, 0).r;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                half4 col = SampleTriplanar(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), i.posWS, i.normalWS);
                half snowH = saturate((i.height - 0.3) * 2.0);
                half4 snow = SampleTriplanar(TEXTURE2D_ARGS(_SnowTex, sampler_SnowTex), i.posWS, i.normalWS);
                col = lerp(col, snow, snowH * _SnowAmount) * _Color;

                float var = hash12(floor(i.posWS.xz / _PixelSize));
                col.rgb *= 1.0 + (var - 0.5) * _ColorVariation;

                half3 lighting = half3(_UnlitBrightness, _UnlitBrightness, _UnlitBrightness);

                #ifndef UNLIT_MODE
                    half3 lightDir = _DirectionalLightDatas[0].forward;
                    half ndotl = saturate(dot(i.normalWS, -lightDir));
                    lighting += _DirectionalLightDatas[0].color * ndotl;
                #endif

                col.rgb *= lighting;

                float2 sp = floor(fmod(i.posCS.xy, 4.0));
                float d = frac(sin(dot(sp * 0.25, float2(12.9898, 78.233))) * 43758.5453);
                col.rgb = floor(col.rgb * 31.0 + d * _DitherStrength) / 31.0;

                return col;
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
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes { float4 posOS:POSITION; };
            struct Varyings { float4 posCS:SV_POSITION; };

            Varyings vert(Attributes v) {
                Varyings o;
                o.posCS = TransformWorldToHClip(TransformObjectToWorld(v.posOS.xyz));
                return o;
            }

            half4 frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
