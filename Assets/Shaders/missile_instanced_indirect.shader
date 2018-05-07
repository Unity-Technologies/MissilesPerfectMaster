Shader "Custom/missile_instanced_indirect" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {
        Pass {
            Tags {"LightMode"="ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _LightColor0; 

            #include "missile.cginc"
            StructuredBuffer<MissileData> cbuffer_missile;
            StructuredBuffer<int> cbuffer_missile_alive_index;
            StructuredBuffer<SortData> cbuffer_missile_sort_key_list;
            float _CurrentTime;

             struct appdata_custom {
                uint instanceID : SV_InstanceID;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2  uv          : TEXCOORD0;
                float3  lightDir    : TEXCOORD1;
                float3  normal        : TEXCOORD2;
                LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
            };

            v2f vert (appdata_custom v)
            {
                int midx = cbuffer_missile_sort_key_list[v.instanceID].packed_&0xffff; // ソート済みリストからインデクス
                float elapsed = _CurrentTime - cbuffer_missile[midx].dead_time_; // 正なら死亡している
                float3 p = cbuffer_missile[midx].position_;
                float4 q = cbuffer_missile[midx].rotation_;
                float4x4 mat = {
                    1 - 2*q.y*q.y - 2*q.z*q.z,     2*q.x*q.y - 2*q.z*q.w,     2*q.x*q.z + 2*q.y*q.w, p.x,
                        2*q.x*q.y + 2*q.z*q.w, 1 - 2*q.x*q.x - 2*q.z*q.z,     2*q.y*q.z - 2*q.x*q.w, p.y,
                        2*q.x*q.z - 2*q.y*q.w,     2*q.y*q.z + 2*q.x*q.w, 1 - 2*q.x*q.x - 2*q.y*q.y, p.z,
                                            0,                         0,                         0,   1,
                };
                float4 wpos = mul(mat, v.vertex);

                v2f o;
                o.pos = UnityObjectToClipPos(float4(wpos.xyz, 1));
                o.pos.z = elapsed > 0 ? -1 : o.pos.z; // 死亡していたら見えないところへ移動
                o.uv = v.texcoord.xy;
                o.lightDir = ObjSpaceLightDir(v.vertex);
                o.normal = v.normal;
                TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow & attenuation to the fragment shader.

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.lightDir = normalize(i.lightDir);
                fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow & attenuation value.
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed diff = saturate(dot(i.normal, i.lightDir));
                fixed4 c;
                // c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * 2 * tex.rgb);         // Ambient term. Only do this in Forward Base. It only needs calculating once.
                c.rgb = (half3(0.1, 0.1, 0.1) * 2 * tex.rgb);
                c.rgb += (tex.rgb * _LightColor0.rgb * diff) * (atten * 2); // Diffuse and specular.
                c.a = 1;
                return c;
            }

            ENDCG
        }
    }
}
