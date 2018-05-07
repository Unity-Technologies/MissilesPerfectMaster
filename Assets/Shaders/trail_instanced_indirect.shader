Shader "Custom/trail_instanced_indirect" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Debug ("Debug", float) = 0
    }
    SubShader {
           Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha // alpha blending
        
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #define SMOOTH_TRAIL

             #include "UnityCG.cginc"
            #if SHADER_TARGET >= 45
            #include "missile.cginc"
            StructuredBuffer<float4> cbuffer_trail;
            StructuredBuffer<int> cbuffer_trail_index;
            StructuredBuffer<SortData> cbuffer_missile_sort_key_list;
            #endif

            float _CurrentTime;
            int _DisplayNum;
            sampler2D _MainTex;
            float _Debug;

             struct appdata_custom {
                uint instanceID : SV_InstanceID;
                uint vertexID : SV_VertexID;
                float4 vertex : POSITION;
#if defined(SHADER_API_PSSL)
                float2 texcoord : TEXCOORD0;
#endif
            };

             struct v2f {
                 float4 pos : SV_POSITION;
                 half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
#if defined(SHADER_API_PSSL)
                float2 texcoord1 : TEXCOORD1;
#endif
             };
             
            v2f vert(appdata_custom v)
            {
                int midx = cbuffer_missile_sort_key_list[(_DisplayNum-1) - v.instanceID].packed_&0xffff; // ソート済みリストからインデクス
                int vidx = v.vertexID/2;                              // 節のインデクス
                int pidx0 = (cbuffer_trail_index[midx]-vidx+TRAIL_LENGTH) & (TRAIL_LENGTH-1); // バッファインデクス描画点
                int idx_dir = (vidx == 0 ? -1 : 1); // ビルボード参照点は最初の点のみ次の点、以降は前の点
                int pidx1 = (pidx0 + idx_dir + TRAIL_LENGTH) & (TRAIL_LENGTH-1); // バッファインデクス参照点
                float4 node = cbuffer_trail[midx*TRAIL_LENGTH + pidx0]; // 描画頂点
                float4 v0 = float4(node.xyz, 1); // 
                float4 v1 = float4(cbuffer_trail[midx*TRAIL_LENGTH + pidx1].xyz, 1); // 参照頂点
                float elapsed = _CurrentTime - node.w; // 噴出されてからの時間
                float size = 0.125+elapsed*0.2; // サイズを時間で拡大
                float3 eye = ObjSpaceViewDir(v0);    // 視線ベクトル
                float3 diff = v1 - v0;                // 差分
                
#if defined(SMOOTH_TRAIL)
                int idx_dir2 = (vidx == TRAIL_LENGTH-1 ? 1 : -1);
                int pidx2 = (pidx0 + idx_dir2 + TRAIL_LENGTH) & (TRAIL_LENGTH-1); // バッファインデクス参照点2
                float4 v2 = float4(cbuffer_trail[midx*TRAIL_LENGTH + pidx2].xyz, 1); // 参照頂点2
                float4 p0 = UnityObjectToClipPos(v1);
                float4 p1 = UnityObjectToClipPos(v0);
                float4 p2 = UnityObjectToClipPos(v2);
                float wpx = p1.w*p0.x - p0.w*p1.x;
                float wpy = p1.w*p0.y - p0.w*p1.y;
                float wnx = p2.w*p1.x - p1.w*p2.x;
                float wny = p2.w*p1.y - p1.w*p2.y;
                float ww = p0.w*p1.w*p2.w*size*0.01;
                float topology_fade = _Debug > 0 ? 1 : smoothstep(0, ww, wpx*wnx+wpy*wny); // ww=1を仮定できないので除算が呼ばれる
#else
                // float topology_fade = step(0.01, dot(diff, diff)); // 距離が狭いときに消す
                float topology_fade = 1;
#endif

                float3 side = normalize(cross(eye, diff)); // ビルボード横ベクトル
                float width_dir = (float)(v.vertexID&1);       // 頂点位置
                v0.xyz += (width_dir-0.5)*idx_dir*side*size; // ビルボード計算
                float2 tex = float2(width_dir, node.w*4); // 発生からの絶対時間
                v2f o;
                o.pos = UnityObjectToClipPos(v0);
                float d = (float)vidx * (1.0/(float)(TRAIL_LENGTH-1)); // 尾の方向にフェードアウト
                o.color = float4(elapsed,                           //  経過時間
                                 1.0,                               //  未使用
                                 1.0,                               //  未使用
                                 ((1.0 - d*d) *
                                  topology_fade *
                                  (vidx == 0 ? 0.0 : 1.0))); // 最初のノードはゼロ
                o.texcoord = tex;
#if defined(SHADER_API_PSSL)
                o.texcoord1 = v.texcoord;
#endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed value = tex2D(_MainTex, i.texcoord).a;
                return fixed4(0.5, 0.5, 0.5, value * i.color.a);
            }

            ENDCG
        }
    }
}
