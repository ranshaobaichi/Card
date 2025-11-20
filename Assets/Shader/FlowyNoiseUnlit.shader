Shader "Custom/FlowyNoiseUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 5.0
        _FlowSpeed ("Flow Speed", Float) = 1.0
        _FlowStrength ("Flow Strength", Float) = 0.1
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.5
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _NoiseScale;
            float _FlowSpeed;
            float _FlowStrength;
            float _BlurAmount;
            float4 _ColorTint;

            // 简单的2D噪声函数
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Perlin噪声
            float perlinNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _FlowSpeed;
                
                // 创建流动的噪声偏移
                float2 noiseUV = i.uv * _NoiseScale;
                float2 flow;
                flow.x = perlinNoise(noiseUV + float2(time, 0)) * 2.0 - 1.0;
                flow.y = perlinNoise(noiseUV + float2(0, time)) * 2.0 - 1.0;
                
                // 应用流动偏移
                float2 distortedUV = i.uv + flow * _FlowStrength;
                
                // 模糊采样（多次采样平均）
                fixed4 col = fixed4(0, 0, 0, 0);
                float samples = 9.0;
                float offset = _BlurAmount * 0.02;
                
                for(float x = -1.0; x <= 1.0; x += 1.0)
                {
                    for(float y = -1.0; y <= 1.0; y += 1.0)
                    {
                        float2 sampleUV = distortedUV + float2(x, y) * offset;
                        col += tex2D(_MainTex, sampleUV);
                    }
                }
                
                col /= samples;
                col *= _ColorTint;
                
                return col;
            }
            ENDCG
        }
    }
}