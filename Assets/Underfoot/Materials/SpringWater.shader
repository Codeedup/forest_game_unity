Shader "Underfoot/Spring Water"
{
    // Lit, fogged water with a soft turquoise shimmer that drifts across the surface.
    Properties
    {
        _Color ("Water colour", Color) = (.37, .72, .69, 1)
        _Glossiness ("Smoothness", Range(0, 1)) = .8
        [HDR] _EmissionColor ("Base glow", Color) = (.08, .13, .13, 1)
        [HDR] _ShimmerColor ("Shimmer", Color) = (.9, 1.6, 1.45, 1)
        _ShimmerScale ("Shimmer scale (per metre)", Float) = 180
        _ShimmerSpeed ("Shimmer speed", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color, _EmissionColor, _ShimmerColor;
        half _Glossiness, _ShimmerScale, _ShimmerSpeed;
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float t = _Time.y * _ShimmerSpeed;
            float2 p = IN.worldPos.xz * _ShimmerScale;
            float w = sin(p.x + t * 1.1) * sin(p.y * 1.3 - t * .9) + sin((p.x + p.y) * .7 + t * 1.6) * .6;
            float shimmer = smoothstep(.35, 1.45, w);
            o.Albedo = _Color.rgb * (.9 + shimmer * .2);
            o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor.rgb + _ShimmerColor.rgb * shimmer * .22;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
