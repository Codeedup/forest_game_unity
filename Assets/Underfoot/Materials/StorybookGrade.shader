Shader "Hidden/Underfoot/Storybook Grade"
{
    Properties { _MainTex ("Source", 2D) = "white" {} }
    CGINCLUDE
    #include "UnityCG.cginc"
    sampler2D _MainTex, _BloomTex;
    float4 _MainTex_TexelSize;
    float4 _Bloom;   // threshold, intensity, knee
    float4 _Grade;   // split toning, saturation, vignette
    float4 _HighlightTint, _ShadowTint, _VignetteColour;

    half3 Box(float2 uv, float spread)
    {
        float4 o = _MainTex_TexelSize.xyxy * float4(-1, -1, 1, 1) * spread;
        return (tex2D(_MainTex, uv + o.xy).rgb + tex2D(_MainTex, uv + o.zy).rgb +
                tex2D(_MainTex, uv + o.xw).rgb + tex2D(_MainTex, uv + o.zw).rgb) * .25;
    }
    half4 Prefilter(v2f_img i) : SV_Target
    {
        half3 c = Box(i.uv, 1);
        half bright = max(c.r, max(c.g, c.b));
        // Soft knee keeps glows smooth instead of a hard cut-off.
        half soft = clamp(bright - _Bloom.x + _Bloom.z, 0, 2 * _Bloom.z);
        soft = soft * soft / (4 * _Bloom.z + 1e-4);
        half contribution = max(soft, bright - _Bloom.x) / max(bright, 1e-4);
        return half4(min(c * contribution, 8), 1);
    }
    half4 Down(v2f_img i) : SV_Target { return half4(Box(i.uv, 1), 1); }
    half4 Up(v2f_img i) : SV_Target { return half4(Box(i.uv, .5), 1); }
    half4 Composite(v2f_img i) : SV_Target
    {
        half3 c = tex2D(_MainTex, i.uv).rgb + tex2D(_BloomTex, i.uv).rgb * _Bloom.y;
        half lum = dot(c, half3(.2126, .7152, .0722));
        // Warm highlights, cool shadows: a multiplicative tint that keeps mid-tones close to neutral.
        half3 tint = lerp(_ShadowTint.rgb, _HighlightTint.rgb, smoothstep(.02, .55, lum));
        c = lerp(c, c * tint * 1.08, _Grade.x);
        c = max(0, lerp(lum.xxx, c, 1 + _Grade.y));
        float2 d = (i.uv - .5) * float2(1.15, 1);
        half v = saturate(pow(dot(d, d) * 2.1, 1.35) * _Grade.z);
        c = lerp(c, _VignetteColour.rgb * c * 2 + _VignetteColour.rgb * .05, v);
        return half4(c, 1);
    }
    ENDCG
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Prefilter
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Down
            ENDCG }
        Pass { Blend One One
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Up
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Composite
            ENDCG }
    }
    Fallback Off
}
