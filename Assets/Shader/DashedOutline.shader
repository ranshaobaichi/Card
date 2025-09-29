Shader "Custom/DashedOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 2
        _DashSize ("Size", Range(0.1, 10)) = 2
        _GapSize ("Gap Size", Range(0.1, 10)) = 1
        _DashOffset ("Offset", Range(0, 10)) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
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
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _DashSize;
            float _GapSize;
            float _DashOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv) * i.color;
                
                // 计算到边缘的距离
                float2 uvDist = abs(i.uv - 0.5) * 2;
                float maxDist = max(uvDist.x, uvDist.y);
                
                // 创建基本描边
                float outlineThreshold = 1.0 - (_OutlineWidth / 100.0);
                float outlineValue = step(outlineThreshold, maxDist);
                
                // 生成虚线模式
                float totalLength = _DashSize + _GapSize;
                
                // 根据uv坐标计算周长位置
                float2 uvPerimeter = i.uv;
                float perimeterCoord;
                
                // 确定当前点位于哪条边上
                if (uvDist.x > uvDist.y) {
                    // 在左边或右边
                    perimeterCoord = i.uv.y;
                    if (i.uv.x < 0.5) perimeterCoord = 1.0 - perimeterCoord; // 左边从下往上
                } else {
                    // 在上边或下边
                    perimeterCoord = i.uv.x;
                    if (i.uv.y > 0.5) perimeterCoord = 1.0 - perimeterCoord; // 上边从右往左
                }
                
                // 应用虚线模式
                float dashPos = fmod(perimeterCoord * 4.0 + _DashOffset, totalLength);
                float dashMask = step(dashPos, _DashSize);
                
                // 只在边框处应用虚线效果
                float finalMask = outlineValue * dashMask;
                
                // 混合原始颜色和描边颜色
                float4 outlineColor = _OutlineColor;
                outlineColor.a *= finalMask;
                color = float4(lerp(color.rgb, outlineColor.rgb, outlineColor.a), max(color.a, outlineColor.a));
                
                return color;
            }
            ENDCG
        }
    }
}