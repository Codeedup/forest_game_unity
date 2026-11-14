Shader "Underfoot/Dusk Sky"
{
    // A tiny gradient skybox: warm horizon glow towards the low sun, lavender to indigo overhead,
    // fog-coloured below the horizon so distant trees melt into the sky, and a few early stars.
    Properties
    {
        _Zenith ("Zenith", Color) = (0.20, 0.22, 0.42, 1)
        _Upper ("Upper sky", Color) = (0.47, 0.45, 0.66, 1)
        _Horizon ("Horizon", Color) = (0.93, 0.66, 0.52, 1)
        _Ground ("Below horizon (match fog)", Color) = (0.36, 0.43, 0.45, 1)
        _SunGlow ("Sun glow", Color) = (1.0, 0.72, 0.42, 1)
        _StarDensity ("Star density", Range(0, 0.01)) = 0.0025
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Zenith, _Upper, _Horizon, _Ground, _SunGlow;
            float _StarDensity;
            struct Input { float4 vertex : POSITION; };
            struct Output { float4 vertex : SV_POSITION; float3 direction : TEXCOORD0; };
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.direction = v.vertex.xyz; return o;
            }
            float Hash(float3 p) { return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453); }
            fixed4 frag(Output i) : SV_Target
            {
                float3 d = normalize(i.direction);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float towardSun = saturate(dot(normalize(float3(d.x, 0, d.z)), normalize(float3(sun.x, 0, sun.z))) * .5 + .5);
                float y = d.y;
                // Warmth gathers on the sun side of the horizon and cools away from it.
                float3 horizon = lerp(lerp(_Upper.rgb, _Horizon.rgb, .55), _Horizon.rgb, towardSun * towardSun);
                float3 sky = lerp(horizon, _Upper.rgb, smoothstep(0.0, 0.32, y));
                sky = lerp(sky, _Zenith.rgb, smoothstep(0.3, 0.95, y));
                sky = lerp(sky, _Ground.rgb, smoothstep(0.1, -0.1, y) * .85 + smoothstep(0.0, -0.15, y) * .15);
                float glow = pow(saturate(dot(d, sun)), 10) * .55 + pow(saturate(dot(d, sun)), 120) * .9;
                sky += _SunGlow.rgb * glow * smoothstep(-0.08, 0.05, y);
                // Sparse, gently varying early stars on the dark side of the sky.
                float3 cell = floor(d * 220);
                float star = step(1 - _StarDensity, Hash(cell)) * smoothstep(0.25, 0.7, y) * (1 - towardSun * .8);
                sky += star * (.5 + Hash(cell + 7) * .7) * float3(1, .95, .85);
                return fixed4(sky, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
