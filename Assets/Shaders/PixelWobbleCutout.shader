Shader "GameJam/UI/Pixel Wobble Cutout"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0.52, 0.68, 1)
        _Speed ("Speed", Range(0, 5)) = 1.2
        _WobbleAmount ("Wobble Amount", Range(0, 1)) = 0.65
        _PixelSize ("Pixel Size", Range(2, 32)) = 8

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
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float _Speed;
            float _WobbleAmount;
            float _PixelSize;
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
                float2 uvDerivative = float2(abs(ddx(IN.texcoord.x)), abs(ddy(IN.texcoord.y)));
                float2 rectPixelSize = 1.0 / max(uvDerivative, 0.0001);
                float2 pixelSteps = max(2.0, rectPixelSize / max(2.0, _PixelSize));
                float2 pixelUv = (floor(IN.texcoord * pixelSteps) + 0.5) / pixelSteps;
                float2 p = pixelUv * 2.0 - 1.0;
                p.y += 0.08;

                float t = _Time.y * _Speed;
                float angle = atan2(p.y, p.x);
                float wave =
                    sin(angle * 3.0 + t * 1.1) * 0.09 +
                    sin(angle * 6.0 - t * 0.8) * 0.05 +
                    sin((pixelUv.x + pixelUv.y) * 18.0 + t * 1.4) * 0.025;

                float2 blobSpace = p * float2(0.92, 1.16);
                float radius = 0.8 + wave * _WobbleAmount;
                float body = radius - length(blobSpace);

                float bottomShelf = pixelUv.y - 0.02;
                float mask = min(body, bottomShelf);

                fixed4 textureColor = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;
                fixed4 color = textureColor * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                clip(min(mask, textureColor.a - 0.5));
                return color;
            }
            ENDCG
        }
    }
}
