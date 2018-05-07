/* -*- mode:Shader coding:utf-8-with-signature -*-
 */

Shader "Custom/skybox" {
Properties {
    [NoScaleOffset] _Tex ("Cubemap", Cube) = "grey" {}
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        ColorMask RGBA
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragSimple
            #pragma target 2.0
            #include "UnityCG.cginc"

            samplerCUBE _Tex;
            struct appdata_t {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;
                return o;
            }
            
            fixed4 fragSimple(v2f i) : SV_Target
            {
                fixed4 tex = fixed4(texCUBE(_Tex, i.texcoord).rgb, 0);
                return tex;
            }

            ENDCG 
        }
    }     
    // Fallback Off
}

/*
 * End of skybox.shader
 */
