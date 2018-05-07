Shader "Custom/burner_instanced_indirect" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {
           Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha One

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            #include "missile.cginc"
            StructuredBuffer<MissileData> cbuffer_missile;
            StructuredBuffer<int> cbuffer_missile_alive_index;
            StructuredBuffer<SortData> cbuffer_missile_sort_key_list;
            float _CurrentTime;

             struct appdata_custom {
                uint instanceID : SV_InstanceID;
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR0;
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
                o.texcoord = v.texcoord;
                o.color = v.color * 8;
                o.color.a = cbuffer_missile[midx].random_value_;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = frac(_CurrentTime) + i.color.a;
                float2 uv = i.texcoord + float2(time*8, time*20);
                return tex2D(_MainTex, uv) * fixed4(i.color.rgb, 1);
            }

            ENDCG
        }
    }
}
