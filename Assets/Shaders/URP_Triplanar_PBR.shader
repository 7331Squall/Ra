Shader "Custom/URP_Triplanar_PBR"
{
    Properties
    {
        _BaseMap ("Diffuse", 2D) = "white" {}
        _GridMap ("GridTexture", 2D) = "GridBox_Default" {}

        _BaseColor ("Diffuse Color", Color) = (128,128,128)

        _NormalMap ("Normal", 2D) = "bump" {}
        _RoughnessMap ("Roughness", 2D) = "white" {}
        _AOMap ("AO", 2D) = "white" {}
        _HeightMap ("Displacement", 2D) = "black" {}

        _NormalStrength ("Normal Strength", Float) = 1
        _HeightStrength ("Height Strength", Float) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;

            TEXTURE2D(_GridMap);
            SAMPLER(sampler_GridMap);
            float4 _GridMap_ST;

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            float4 _NormalMap_ST;

            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);
            float4 _RoughnessMap_ST;

            TEXTURE2D(_AOMap);
            SAMPLER(sampler_AOMap);
            float4 _AOMap_ST;

            TEXTURE2D(_HeightMap);
            SAMPLER(sampler_HeightMap);
            float4 _HeightMap_ST;

            float _NormalStrength;
            float _HeightStrength;

            float3 _BaseColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3   worldPos = TransformObjectToWorld(v.positionOS.xyz);

                // Simple displacement (vertex)
                float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, v.positionOS.xz *  _HeightMap_ST.xy + _HeightMap_ST.zw, 0).r;
                worldPos += normalize(TransformObjectToWorldNormal(v.normalOS)) * (height * _HeightStrength);

                o.positionCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float3 TriplanarBlend(float3 normal)
            {
                float3 blend = abs(normal);
                blend /= (blend.x + blend.y + blend.z);
                return blend;
            }

            float4 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float4 stCoords, float3 worldPos, float3 blend)
            {
                float2 uvX = worldPos.yz * stCoords.xy + stCoords.zw;
                float2 uvY = worldPos.xz * stCoords.xy + stCoords.zw;
                float2 uvZ = worldPos.xy * stCoords.xy + stCoords.zw;

                float4 x = SAMPLE_TEXTURE2D(tex, samp, uvX);
                float4 y = SAMPLE_TEXTURE2D(tex, samp, uvY);
                float4 z = SAMPLE_TEXTURE2D(tex, samp, uvZ);

                return x * blend.x + y * blend.y + z * blend.z;
            }

            float3 SampleTriplanarNormal(float3 worldPos, float3 worldNormal, float3 blend)
            {
                float2 uvX = worldPos.yz * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float2 uvY = worldPos.xz * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float2 uvZ = worldPos.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;

                float3 nX = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvX));
                float3 nY = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvY));
                float3 nZ = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvZ));

                // Swizzle for axis alignment
                nX = float3(0, nX.y, nX.x);
                nY = float3(nY.x, 0, nY.y);
                nZ = float3(nZ.x, nZ.y, 0);

                float3 normal = nX * blend.x + nY * blend.y + nZ * blend.z;
                return normalize(lerp(worldNormal, normal, _NormalStrength));
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 blend = TriplanarBlend(normal);

                float4 albedo = SampleTriplanar(_BaseMap, sampler_BaseMap, _BaseMap_ST, i.worldPos, blend);
                float4 grid = SampleTriplanar(_GridMap, sampler_GridMap, _GridMap_ST, i.worldPos, blend);
                float  roughness = SampleTriplanar(_RoughnessMap, sampler_RoughnessMap, _RoughnessMap_ST, i.worldPos, blend).r;
                float  ao = SampleTriplanar(_AOMap, sampler_AOMap, _AOMap_ST, i.worldPos, blend).r;

                float3 finalNormal = SampleTriplanarNormal(i.worldPos, normal, blend);

                Light  light = GetMainLight();
                float3 lightDir = normalize(light.direction);

                float  NdotL = saturate(dot(finalNormal, -lightDir));
                float3 diffuse = albedo.rgb * light.color * NdotL;

                float3 ambient = albedo.rgb * ao * 0.3;

                float3 color = (diffuse + ambient) * (_BaseColor * 2) * grid;


                return float4(color, 1);
            }
            ENDHLSL
        }
    }
}