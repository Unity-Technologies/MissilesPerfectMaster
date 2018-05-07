// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Image Effects/Cinematic/Bloom"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
        _BaseTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _BaseTex;
    float2 _MainTex_TexelSize;
    float2 _BaseTex_TexelSize;

    float _PrefilterOffs;
    half _Threshold;
    half _RCutoff;
    float _SampleScale;
    half _Intensity;

    // Luma function with Rec.709 HDTV Standard
    inline half Luma(half3 c)
    {
        return dot(c, half3(0.2126, 0.7152, 0.0722));
    }

    // RGBM encoding/decoding
    inline half4 EncodeHDR(half3 rgb)
    {
        return half4(rgb, 0);
    }

    inline half3 DecodeHDR(half4 rgba)
    {
        return rgba.rgb;
    }

    // Downsample with a 4x4 box filter
    inline half3 DownsampleFilter(float2 uv)
    {
        float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);
        half3 s;
        s  = DecodeHDR(tex2D(_MainTex, uv + d.xy));
        s += DecodeHDR(tex2D(_MainTex, uv + d.zy));
        s += DecodeHDR(tex2D(_MainTex, uv + d.xw));
        s += DecodeHDR(tex2D(_MainTex, uv + d.zw));
        return s * (1.0 / 4);
    }

    inline half3 UpsampleFilter(float2 uv)
    {
        // 4-tap bilinear upsampler
        float4 d = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1) * (_SampleScale * 0.5);
        half3 s;
        s  = DecodeHDR(tex2D(_MainTex, uv + d.xy));
        s += DecodeHDR(tex2D(_MainTex, uv + d.zy));
        s += DecodeHDR(tex2D(_MainTex, uv + d.xw));
        s += DecodeHDR(tex2D(_MainTex, uv + d.zw));
        return s * (1.0 / 4);
    }

    //
    // Vertex shader
    //

    struct v2f_multitex
    {
        float4 pos : SV_POSITION;
        float2 uvMain : TEXCOORD0;
        float2 uvBase : TEXCOORD1;
    };

    v2f_multitex vert_multitex(appdata_full v)
    {
        v2f_multitex o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uvMain = v.texcoord.xy;
        o.uvBase = v.texcoord.xy;
    #if UNITY_UV_STARTS_AT_TOP
        if (_BaseTex_TexelSize.y < 0.0)
            o.uvBase.y = 1.0 - v.texcoord.y;
    #endif
        return o;
    }

    //
    // fragment shader
    //
    half4 frag_prefilter(v2f_img i) : SV_Target
    {
        float2 uv = i.uv + _MainTex_TexelSize.xy * _PrefilterOffs;

        // half4 s0 = SafeHDR(tex2D(_MainTex, uv));
        half4 s0 = tex2D(_MainTex, uv);
        half3 m = s0.rgb;

        half lm = Luma(m);
        m *= saturate((lm - _Threshold) * _RCutoff);

        return EncodeHDR(m);
    }

    half4 frag_downsample1(v2f_img i) : SV_Target
    {
        return EncodeHDR(DownsampleFilter(i.uv));
    }

    half4 frag_downsample2(v2f_img i) : SV_Target
    {
        return EncodeHDR(DownsampleFilter(i.uv));
    }

    half4 frag_upsample(v2f_multitex i) : SV_Target
    {
        half3 base = DecodeHDR(tex2D(_BaseTex, i.uvBase));
        half3 blur = UpsampleFilter(i.uvMain);
        return EncodeHDR(base + blur);
    }

    half4 frag_upsample_final(v2f_multitex i) : SV_Target
    {
        half4 base = tex2D(_BaseTex, i.uvBase);
        half3 blur = UpsampleFilter(i.uvMain);
        half3 cout = base.rgb + blur * _Intensity;
        return half4(cout, base.a);
    }

    ENDCG
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_prefilter
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_downsample1
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_downsample2
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_multitex
            #pragma fragment frag_upsample
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_multitex
            #pragma fragment frag_upsample_final
            #pragma target 3.0
            ENDCG
        }
    }
}
