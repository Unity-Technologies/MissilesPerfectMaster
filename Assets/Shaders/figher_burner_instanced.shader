Shader "Custom/fighter_burner_instanced"
{
    Properties 
    {
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
            
            struct appdata_custom
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4  pos         : SV_POSITION;
            };
            
            v2f vert(appdata_custom v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : COLOR
            {
                return fixed4(0.5, 0.75, 1, 1);
            }
            ENDCG
        }
    }
    FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}
/*
 * End of fighter.shader
 */
