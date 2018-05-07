/* -*- mode:Shader coding:utf-8-with-signature -*-
 */
Shader "Custom/fighter_instanced"
{
    Properties 
    {
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    }
    SubShader 
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}

        Pass 
        {
            Tags {"LightMode" = "ForwardBase"}                      // This Pass tag is important or Unity may not give it the correct light information.

               CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase                       // This line tells Unity to compile this pass for forward base.
            #pragma multi_compile_instancing
           
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            
            struct appdata_custom
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4  pos         : SV_POSITION;
                float2  uv          : TEXCOORD0;
                float3  lightDir    : TEXCOORD1;
                float3  normal        : TEXCOORD2;
                LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _LightColor0; 
            
            v2f vert(appdata_custom v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                o.lightDir = ObjSpaceLightDir(v.vertex);
                o.normal = v.normal;
                TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow & attenuation to the fragment shader.
                return o;
            }
            
            fixed4 frag(v2f i) : COLOR
            {
                i.lightDir = normalize(i.lightDir);
                fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow & attenuation value.
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed diff = saturate(dot(i.normal, i.lightDir));
                fixed4 c;
                c.rgb = (half3(0.01, 0.01, 0.01) * 2 * tex.rgb);
                c.rgb += (tex.rgb * _LightColor0.rgb * diff * 2) * (atten * 2); // Diffuse and specular.
                c.a = 1;
                return c;
            }
            ENDCG
        }
    }
    FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}
/*
 * End of fighter.shader
 */
