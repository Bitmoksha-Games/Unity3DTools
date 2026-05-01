Shader "BitMoksha/Unlit/ShdrInstancedMeshGen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
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
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
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
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float4x4 _ObjectToWorld;
            uniform float _NumInstances;
            float4 _Color;


            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                PerInstanceData data = _PerInstanceData[instanceID];
                float4 pos = v.vertex;

                float4 wpos = mul(mul(_ObjectToWorld, data.m), v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, wpos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(mul(data.m, v.normal));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                float3 normal = i.normal; //facing > 0 ? i.normal : -i.normal;
                fixed3 lightDir = _WorldSpaceLightPos0;
                fixed3 lightColor = _LightColor0.rgb;
                col *= fixed4(saturate(dot(normal, lightDir)) * lightColor, col.w);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
