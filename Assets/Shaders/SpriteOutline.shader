Shader "Custom/SpriteDoubleOutline_V3_Safe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        // --- アウトライン設定 ---
        [Toggle(_OUTLINE_ON)] _OutlineOn ("Outline On?", Float) = 1
        
        // 内側の設定
        _InnerColor ("Inner Color", Color) = (1,1,1,1)
        _InnerWidth ("Inner Width", Range(0, 50)) = 2.0

        // 外側の設定
        _OuterColor ("Outer Glow Color", Color) = (0,0.5,1,1)
        _OuterWidth ("Outer Glow Width", Range(0, 50)) = 10.0
        
        // 品質の調整
        _Detail ("Quality (Iterations)", Range(5, 30)) = 15 
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma shader_feature _OUTLINE_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _InnerColor;
            float _InnerWidth;
            fixed4 _OuterColor;
            float _OuterWidth;
            float _Detail;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // ここはループ外なので通常のtex2DでOK
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                #ifndef _OUTLINE_ON
                    return c;
                #endif

                if (c.a > 0.95) return c;

                float2 texelSize = _MainTex_TexelSize.xy;
                float totalWidthPixels = _InnerWidth + _OuterWidth;
                int steps = (int)_Detail;

                // コンパイラ対策：最大ループ回数を定数で指定
                for (int i = 1; i <= 30; i++)
                {
                    if (i > steps) break;

                    float distRatio = (float)i / steps;
                    float currentDist = totalWidthPixels * distRatio;
                    float2 offset = texelSize * currentDist;

                    float d = 0.707;
                    fixed a = 0;

                    // 【修正点】
                    // ループ内でのサンプリングエラーを防ぐため、tex2Dlod を使用。
                    // 第2引数は float4(u, v, 0, 0) とすることでミップマップレベルを0に固定します。
                    
                    a += tex2Dlod(_MainTex, float4(IN.texcoord + float2(0, offset.y), 0, 0)).a;
                    a += tex2Dlod(_MainTex, float4(IN.texcoord - float2(0, offset.y), 0, 0)).a;
                    a += tex2Dlod(_MainTex, float4(IN.texcoord + float2(offset.x, 0), 0, 0)).a;
                    a += tex2Dlod(_MainTex, float4(IN.texcoord - float2(offset.x, 0), 0, 0)).a;
                    
                    a += tex2Dlod(_MainTex, float4(IN.texcoord + float2(offset.x * d, offset.y * d), 0, 0)).a;
                    a += tex2Dlod(_MainTex, float4(IN.texcoord + float2(-offset.x * d, offset.y * d), 0, 0)).a;
                    a += tex2Dlod(_MainTex, float4(IN.texcoord + float2(offset.x * d, -offset.y * d), 0, 0)).a;
                    a += tex2Dlod(_MainTex, float4(IN.texcoord + float2(-offset.x * d, -offset.y * d), 0, 0)).a;

                    if (a > 0)
                    {
                        if (currentDist <= _InnerWidth)
                        {
                            return fixed4(_InnerColor.rgb, _InnerColor.a * IN.color.a);
                        }
                        else
                        {
                            float glowPos = (currentDist - _InnerWidth) / _OuterWidth;
                            float alphaFactor = 1.0 - smoothstep(0.0, 1.0, glowPos);
                            return fixed4(_OuterColor.rgb, _OuterColor.a * alphaFactor * IN.color.a);
                        }
                    }
                }

                return c;
            }
            ENDCG
        }
    }
}