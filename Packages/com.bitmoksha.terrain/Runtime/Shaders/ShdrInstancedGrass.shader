Shader "BitMoksha/Unlit/ShdrInstancedGrass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopColor("Top Color", Color) = (0, 1, 0, 1)
        _BottomColor("Bottom Color", Color) = (0, 0.5, 0, 1)
        _BendRange("Bend Range", Range(0, 1)) = 0.2
        _WindDistortionMap("Wind Distortion", 2D) = "white" {}
        _WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
        _WindStrength("Wind Strength", float) = 1.0
    }

    CGINCLUDE

    #include "UtilityFunctions.cginc"

    struct PerInstanceData {
	    float4x4 m;
    };
    StructuredBuffer<PerInstanceData> _PerInstanceData;

    ENDCG

    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "LightMode" = "UniversalForward"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal: NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4x4 _ObjectToWorld;
            uniform float _NumInstances;
            float4 _TopColor;
            float4 _BottomColor;
            float _BendRange;
            sampler2D _WindDistortionMap;
            float4 _WindDistortionMap_ST;
            float2 _WindFrequency;
            float _WindStrength;


            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                PerInstanceData data = _PerInstanceData[instanceID];
                float4 pos = v.vertex;

                float4 tangent = v.tangent;
                float3 normal = v.normal;
                float3 binormal = cross(normal, tangent) * tangent.w;
                float3x3 tangentToLocal = float3x3(
                    tangent.x, binormal.x, normal.x, 
                    tangent.y, binormal.y, normal.y,  
                    tangent.z, binormal.z, normal.z
                );

                float2 uv = float2(data.m[0][3], data.m[2][3]) * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
                float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength * pos.y;
                float3 wind = normalize(float3(windSample.x, windSample.y, 0));
                float3x3 windRotation = AngleAxis3x3(UNITY_PI * windSample, wind);
                float3x3 bendMatrix = AngleAxis3x3(pos.y * _BendRange * UNITY_PI * 0.5, float3(-1.0, 0.0, 0.0));
                pos = float4(mul(mul(bendMatrix, windRotation), pos.xyz), pos.w);

                float4 wpos = mul(mul(_ObjectToWorld, data.m), pos);
                o.vertex = mul(UNITY_MATRIX_VP, wpos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(mul(data.m, v.normal));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 normal = i.normal; //facing > 0 ? i.normal : -i.normal;
                fixed3 lightDir = _WorldSpaceLightPos0;
                fixed3 lightColor = _LightColor0.rgb;
                fixed3 ambient = ShadeSH9(fixed4(i.normal, 1));

                // col *= fixed4(saturate(dot(normal, lightDir)) * lightColor, col.w)
                //     + fixed4(saturate(dot(-normal, lightDir)) * lightColor * 0.2, col.w);
                col *= lerp(_BottomColor, _TopColor, i.uv.y);
                col *= fixed4(saturate(dot(normal, lightDir)) * lightColor, col.w);
                // col *= fixed4(saturate(dot(normal, lightDir)) * lightColor, col.w);
                // Light light = GetMainLight();
                // col *= LightingLambert(light.color, light.direction, normal);
                // col += fixed4(ambient, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
