Shader "Underfoot/Soft Glow"
{
    // Fake light, not real-time lights: additive radial glows for fireflies and motes (Shape 0)
    // soft-edged beams for canopy light shafts (Shape 1) or ripple rings (Shape 2). Fogs towards black.
    Properties
    {
        [HDR] _Color ("Glow colour", Color) = (1, .85, .5, 1)
        _Shape ("Shape: 0 radial, 1 shaft, 2 ripple ring", Range(0, 2)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One ZWrite Off Cull Off Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _Shape;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; UNITY_FOG_COORDS(1) };
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.vertex); return o;
            }
            fixed4 frag(Output i) : SV_Target
            {
                float d = length(i.uv - .5) * 2;
                float radial = pow(saturate(1 - d), 2.2) + smoothstep(.35, 0, d) * .8;
                float across = sin(saturate(i.uv.x) * 3.14159); across *= across;
                float along = smoothstep(0, .35, i.uv.y) * smoothstep(1, .55, i.uv.y);
                float ring = smoothstep(.55, .8, d) * smoothstep(1, .84, d);
                float shape = _Shape < .5 ? radial : (_Shape < 1.5 ? across * along : ring);
                fixed4 c = _Color * i.color; c.rgb *= c.a * shape;
                UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(0, 0, 0, 0));
                return fixed4(c.rgb, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
