Shader "Custom/explosion_instanced_indirect" {
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
            StructuredBuffer<int> cbuffer_explosion_alive_index;
            StructuredBuffer<SortData> cbuffer_missile_sort_key_list;
            float _CurrentTime;
            float3 _CamUp;

             struct appdata_custom {
                uint instanceID : SV_InstanceID;
                uint vertexID : SV_VertexID;
                float4 vertex : POSITION;
#if defined(SHADER_API_PSSL)
                float2 texcoord : TEXCOORD0;
#endif
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
#if defined(SHADER_API_PSSL)
                float2 texcoord1 : TEXCOORD1;
#endif
            };

            v2f vert(appdata_custom v)
            {
                int midx = cbuffer_missile_sort_key_list[v.instanceID].packed_&0xffff; // ソート済みリストからインデクス

                float4 tv = float4(cbuffer_missile[midx].position_, 1);
                float elapsed = _CurrentTime - cbuffer_missile[midx].dead_time_;
                half size = 4;
                size *= step(0, elapsed);
                size *= step(elapsed, 1);
                half3 up = _CamUp;
                half3 eye = normalize(ObjSpaceViewDir(tv));
                half3 side = cross(eye, up);

                // rotate
                float vx = (float)(v.vertexID&1);
                float vy = (float)(v.vertexID/2);
                half3 vec = ((vx-0.5)*side + (vy-0.5)*up)*size;
                half theta = cbuffer_missile[midx].random_value_ * (3.141592*2);
                /* rotate matrix for an arbitrary axis
                 * Vx*Vx*(1-cos) + cos      Vx*Vy*(1-cos) - Vz*sin    Vz*Vx*(1-cos) + Vy*sin;
                 * Vx*Vy*(1-cos) + Vz*sin    Vy*Vy*(1-cos) + cos     Vy*Vz*(1-cos) - Vx*sin;
                 * Vz*Vx*(1-cos) - Vy*sin    Vy*Vz*(1-cos) + Vx*sin    Vz*Vz*(1-cos) + cos;
                 */
                half s, c;
                sincos(theta, s, c);
                half3 n = eye;
                half3 n1c = n * (1-c);
                half3 ns = n * s;
                half3x3 mat = {
                    (n.x*n1c.x + c),   (n.x*n1c.y - ns.z), (n.z*n1c.x + ns.y),
                    (n.x*n1c.y + ns.z), (n.y*n1c.y + c),   (n.y*n1c.z - ns.x),
                    (n.z*n1c.x - ns.y), (n.y*n1c.z + ns.x),   (n.z*n1c.z + c),
                };
                half3 rvec = mul(mat, vec);
                float3 wpos = tv.xyz + rvec;

                float rW = 1.0/8.0;
                float rH = 1.0/8.0;
                float fps = 45;
                float loop0 = 1.0/(fps*rW*rH);
                elapsed = clamp(elapsed, 0, loop0);
                float texu = floor(elapsed*fps) * rW - floor(elapsed*fps*rW);
                float texv = 1 - floor(elapsed*fps*rW) * rH;
                texu += vx * rW;
                texv += -vy * rH;

                v2f o;
                o.pos = UnityObjectToClipPos(float4(wpos, 1));
                o.pos.z = elapsed < 0 ? -1 : o.pos.z; // 死んでいたら見えないところへ移動
                o.texcoord = float2(texu, texv);
#if defined(SHADER_API_PSSL)
                o.texcoord1 = v.texcoord;
#endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.texcoord);
            }

            ENDCG
        }
    }
}
