/* -*- mode:Shader coding:utf-8-with-signature -*-
 */

Shader "Custom/debris" {
    Properties {
    }
    SubShader {
           Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha One                 // alpha additive
        ColorMask RGB
        
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
             #pragma target 3.0
             
             #include "UnityCG.cginc"

             struct appdata_custom {
                float4 vertex : POSITION;
                float4 color : COLOR;
                uint vertexID : SV_VertexID;
            };

             struct v2f {
                 float4 pos:SV_POSITION;
                 fixed4 color:COLOR;
             };
             
             float4x4 _PrevInvMatrix;
            float3   _TargetPosition;
            float    _Range;
            float    _RangeR;
   
            v2f vert(appdata_custom v)
            {
                float3 target = _TargetPosition;
                float3 diff = target - v.vertex.xyz;
                float3 trip = floor( (diff*_RangeR + 1) * 0.5 );
                trip *= (_Range * 2);
                v.vertex.xyz += trip;

                float even = (v.vertexID&1) == 0 ? 1 : 0;

                float4 tv0 = v.vertex;
                tv0 = UnityObjectToClipPos(tv0);
                tv0 *= even;
                
                float4 tv1 = v.vertex;
                tv1 = float4(UnityObjectToViewPos(tv1), 1);
                tv1 = mul(_PrevInvMatrix, tv1);
                tv1 = mul(UNITY_MATRIX_P, tv1);
                tv1.y -= 0.04*(1-even);
                tv1 *= 1-even;
                
                v2f o;
                o.pos = tv0 + tv1;
                float alpha = v.color.a * 0.5;
                o.color = fixed4(1, 1, 1, alpha);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }

            ENDCG
        }
    }
}

/*
 * End of debris.shader
 */
