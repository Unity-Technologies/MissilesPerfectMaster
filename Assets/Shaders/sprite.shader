/* -*- mode:Shader coding:utf-8-with-signature -*-
 */

Shader "Custom/sprite" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        //_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    }
    SubShader {
           Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha // alpha blending
        
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
             #pragma target 3.0
             
             #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform fixed4 _Colors[13];

             struct appdata_custom {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };

             struct v2f {
                 float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                 fixed4 color:COLOR;
             };
   
            v2f vert(appdata_custom v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(float4(v.vertex.xy, 1, 1));
                o.texcoord = MultiplyUV(UNITY_MATRIX_TEXTURE0,
                                        float4(v.texcoord.xy, 0, 0));
                o.texcoord = v.texcoord.xy;
                int color_index = (int)v.vertex.z;
                o.color = _Colors[color_index];
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                return col;
            }

            ENDCG
        }
    }
}

/*
 * End of sprite.shader
 */
