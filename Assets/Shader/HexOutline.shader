Shader "Custom/HexOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0,0.5)) = 0.03
        _HexRadius ("Hex Radius (0..0.5)", Range(0.01,0.5)) = 0.45
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x = 1/w, y = 1/h, z = w, w = h
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _HexRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Signed distance to a regular hexagon centered at origin with circumradius r.
            // UV coordinates expected centered (0,0) and aspect-corrected.
            static float sdHexagon(float2 p, float r)
            {
                p = abs(p);
                float k = dot(p, float2(0.5, 0.86602540378)); // sqrt(3)/2 = 0.8660254
                return max(k, p.x) - r;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture
                fixed4 tex = tex2D(_MainTex, i.uv);
                // center UV at (0,0)
                float2 uv = i.uv - 0.5;

                // Correct aspect so hex is not stretched (texture width/height)
                float aspect = _MainTex_TexelSize.z / _MainTex_TexelSize.w; // width / height
                uv.x *= aspect;

                // Scale UV so that typical hex radius maps to _HexRadius (0..0.5 in centered UV)
                // Here uv in [-0.5,0.5], so _HexRadius=0.45 is near full tile.
                float2 p = uv;
                float sd = sdHexagon(p, _HexRadius);

                // anti-alias width
                float aa = fwidth(sd) * 0.5;

                // inside mask (1 inside hex, 0 outside), with AA
                float inside = smoothstep(aa, -aa, sd);

                // outline mask: where distance to edge (abs(sd)) <= _OutlineThickness
                float outlineRaw = smoothstep(_OutlineThickness + aa, _OutlineThickness - aa, abs(sd));
                // don't let outline cover the main fill (optional): keep outline only outside fill region
                // If you prefer outline over fill, remove the (1 - inside) multiplication
                float outline = outlineRaw * (1.0 - inside);

                // combine texture alpha with inside mask
                float finalAlpha = max(tex.a * inside, _OutlineColor.a * outline);

                // final color: use sprite texture * tint for interior; outline color over it where outline==1
                float3 fillColor = tex.rgb * _Color.rgb;
                float3 color = lerp(fillColor, _OutlineColor.rgb, outline);

                return float4(color, finalAlpha);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}