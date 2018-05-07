/* -*- mode:Shader coding:utf-8-with-signature -*-
 */

#include "UnityCG.cginc"

samplerCUBE _Tex;
// float _SwitchSkybox;

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
    // o.texcoord.y = (step(0.5, 1-_SwitchSkybox)*2-1) * (abs(o.texcoord.y)+0.002 /* remove artifact on horizon. */);
    return o;
}

fixed4 fragSimple(v2f i) : SV_Target
{
    fixed4 tex = fixed4(texCUBE(_Tex, i.texcoord).rgb, 0);
    return tex;
}

// v2f vertMix(appdata_t v)
// {
//     v2f o;
//     o.vertex = UnityObjectToClipPos(v.vertex);
//     o.texcoord = v.vertex.xyz;
//     o.texcoord.y = abs(o.texcoord.y)+0.002 /* remove artifact on horizon. */;
//     return o;
// }

// fixed4 fragMix(v2f i) : SV_Target
// {
//     fixed4 tex0 = texCUBE(_Tex, i.texcoord);
//     half3 t = float3(i.texcoord.x, -i.texcoord.y, i.texcoord.z);
//     fixed4 tex1 = texCUBE(_Tex, t);
//     fixed4 tex = fixed4(lerp(tex0.rgb, tex1.rgb, _SwitchSkybox), 0);
//     return tex;
// }

/*
 * End of skybox.cginc
 */
