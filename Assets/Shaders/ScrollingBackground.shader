Shader "Custom/ScrollingBackground"
{
    Properties
    {
        [Header(Textures)]
        _MainTex        ("Image A", 2D)                     = "white" {}
        _SecondTex      ("Image B", 2D)                     = "white" {}
        _Color          ("Tint", Color)                     = (1,1,1,1)
        _BgColor        ("Background (Gap) Color", Color)   = (0,0,0,0)

        [Header(Grid Layout)]
        _Columns        ("Columns (horizontal tile count)", Float) = 5
        _Rows           ("Rows    (vertical   tile count)", Float) = 5
        _Gap            ("Gap (0~0.5, fraction of cell)",  Range(0, 0.45)) = 0.08

        [Header(Scroll)]
        _ScrollSpeed    ("Speed",  Float)                   = 0.3
        _ScrollAngle    ("Direction (degrees)", Float)       = 45

        [Header(Image Ratio)]
        _RatioA         ("Image A ratio (0=all B, 1=all A)", Range(0, 1)) = 0.5
        _PatternSeed    ("Pattern Seed", Float)             = 0

        [Header(Unity UI Stencil)]
        _StencilComp    ("Stencil Comparison", Float)       = 8
        _Stencil        ("Stencil ID",         Float)       = 0
        _StencilOp      ("Stencil Operation",  Float)       = 0
        _StencilWriteMask("Stencil Write Mask", Float)      = 255
        _StencilReadMask ("Stencil Read Mask",  Float)      = 255
        _ColorMask      ("Color Mask",          Float)      = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull      Off
        Lighting  Off
        ZWrite    Off
        ZTest     [unity_GUIZTestMode]
        Blend     SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            /* ---- properties ---- */
            sampler2D _MainTex;
            sampler2D _SecondTex;
            fixed4    _Color;
            fixed4    _BgColor;

            float _Columns;
            float _Rows;
            float _Gap;

            float _ScrollSpeed;
            float _ScrollAngle;

            float _RatioA;
            float _PatternSeed;

            float4 _ClipRect;            // Unity UI clipping rect

            /* ---- structs ---- */
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                fixed4 color    : COLOR;
            };

            /* ---- vertex ---- */
            v2f vert(appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = v.uv;
                o.worldPos = v.vertex;
                o.color    = v.color * _Color;
                return o;
            }

            /* ---- helpers ---- */

            // Deterministic hash  — returns 0‥1 from 2‑D integer cell index
            float hash21(float2 p)
            {
                p += float2(_PatternSeed, _PatternSeed * 0.7123);
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }

            /* ---- fragment ---- */
            fixed4 frag(v2f i) : SV_Target
            {
                /* --- 1. Build scrolling grid UV --- */
                float2 gridUV = i.uv * float2(_Columns, _Rows);

                float rad = _ScrollAngle * 0.01745329251; // deg → rad
                float2 dir = float2(cos(rad), sin(rad));
                gridUV += dir * _Time.y * _ScrollSpeed;

                /* --- 2. Cell index & local coordinate --- */
                float2 cellIdx = floor(gridUV);
                float2 local   = frac(gridUV);        // 0‥1 inside the cell

                /* --- 3. Gap test (border region → background color) --- */
                float gapHalf = _Gap * 0.5;
                bool inGap = (local.x < gapHalf) || (local.x > 1.0 - gapHalf)
                          || (local.y < gapHalf) || (local.y > 1.0 - gapHalf);

                if (inGap)
                {
                    fixed4 bg = _BgColor;
                    #ifdef UNITY_UI_CLIP_RECT
                    bg.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                    #endif
                    return bg;
                }

                /* --- 4. Remap local UV to 0‥1 within the image area --- */
                float2 imageUV = (local - gapHalf) / (1.0 - _Gap);
                // flip Y so image is upright when UV‑Y goes bottom→top
                imageUV.y = 1.0 - imageUV.y;

                /* --- 5. Choose image A or B based on hash & ratio --- */
                float h = hash21(cellIdx);
                fixed4 col;
                if (h < _RatioA)
                    col = tex2D(_MainTex, imageUV);
                else
                    col = tex2D(_SecondTex, imageUV);

                col *= i.color;

                /* --- 6. Unity UI clipping --- */
                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
