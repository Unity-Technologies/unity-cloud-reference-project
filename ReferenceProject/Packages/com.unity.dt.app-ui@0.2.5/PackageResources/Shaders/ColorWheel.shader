Shader "Hidden/App UI/ColorWheel"
{
    Properties
    {
        _CheckerColor1 ("Color 1", Color) = (1,1,1,1)
        _CheckerColor2 ("Color 2", Color) = (1,1,1,1)
        _CheckerSize ("Size", Float) = 10
        _Width ("Width", Float) = 200
        _Height ("Height", Float) = 200

        _InnerRadius ("Inner Radius", Range(0, 0.5)) = 0.4
        _Saturation ("Saturation", Range(0, 1)) = 1.0
        _Brightness ("Brightness", Range(0,1)) = 1.0
        _Opacity ("Opacity", Range(0,1)) = 1.0
        _AA ("Anti-Aliasing", Float) = 0.005
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AppUI.cginc"

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5;
                return o;
            }

            fixed4 _CheckerColor1;
            fixed4 _CheckerColor2;
            float _CheckerSize;
            float _Width;
            float _Height;

            float _InnerRadius;
            float _Saturation;
            float _Brightness;
            float _Opacity;
            float _AA;

            fixed4 frag (v2f i) : SV_Target
            {
                const float radius = length(i.uv.xy);

                const float mask = smoothstep(0.5, 0.5 - _AA, radius) * smoothstep(_InnerRadius - _AA, _InnerRadius, radius);

                const float angle = atan2(i.uv.y, i.uv.x) * UNITY_INV_TWO_PI;

                fixed4 checker = checker_board(i.uv, _Width, _Height, _CheckerSize, _CheckerColor1, _CheckerColor2);

                float3 hsv = hsv_to_rgb(float3(angle, _Saturation, _Brightness));

                #ifndef UNITY_COLORSPACE_GAMMA
                hsv = GammaToLinearSpace(hsv);
                #endif

                fixed4 color = lerp(checker, fixed4(hsv, mask), _Opacity);
                color.a *= mask;
                return color;
            }
            ENDCG
        }
    }
}
