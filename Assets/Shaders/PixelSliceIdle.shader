Shader "GameJam/UI/Pixel Slice Idle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Speed ("Speed", Range(0, 2)) = 0.18
        _SliceOffsetPixels ("Slice Offset Pixels", Range(0, 8)) = 2
        _SliceHeight ("Slice Height", Range(2, 32)) = 8
        _Frequency ("Frequency", Range(0.1, 4)) = 1.25

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float _Speed;
            float _SliceOffsetPixels;
            float _SliceHeight;
            float _Frequency;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 rectPixelSize = 1.0 / max(float2(abs(ddx(uv.x)), abs(ddy(uv.y))), 0.0001);
                float sliceCount = max(1.0, rectPixelSize.y / max(2.0, _SliceHeight));
                float sliceIndex = floor(uv.y * sliceCount);
                float t = _Time.y * _Speed;

                float wave =
                    sin((sliceIndex * _Frequency) + t) * 0.75 +
                    sin((sliceIndex * 0.37) - (t * 1.63)) * 0.25;

                float texelOffset = round(wave * _SliceOffsetPixels) * _MainTex_TexelSize.x;
                uv.x += texelOffset;
                uv = (floor(uv / _MainTex_TexelSize.xy) + 0.5) * _MainTex_TexelSize.xy;
                uv = saturate(uv);

                fixed4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
