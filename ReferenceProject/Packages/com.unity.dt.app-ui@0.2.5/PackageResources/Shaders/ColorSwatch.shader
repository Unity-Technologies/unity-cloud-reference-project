Shader "Hidden/App UI/ColorSwatch"
{
    Properties
    {
        _CheckerColor1 ("Color 1", Color) = (1,1,1,1)
        _CheckerColor2 ("Color 2", Color) = (1,1,1,1)
        _CheckerSize ("Size", Float) = 10
        _Width ("Width", Float) = 200
        _Height ("Height", Float) = 200
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

            fixed4 _CheckerColor1;
            fixed4 _CheckerColor2;
            float _CheckerSize;
            float _Width;
            float _Height;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = checker_board(i.uv, _Width, _Height, _CheckerSize, _CheckerColor1, _CheckerColor2);
                return color;
            }
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

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

            #define MAX_GRADIENT_COLORS_COUNT 11

            uniform int _Count;
            uniform fixed4 _Colors[MAX_GRADIENT_COLORS_COUNT];
            uniform float _Positions[MAX_GRADIENT_COLORS_COUNT];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const int count = min(_Count, MAX_GRADIENT_COLORS_COUNT);

                clip(count - 1);

                const float current_position = i.uv.x;

                // colors must be sorted by position to work

                int first_color_index = 0;

                UNITY_UNROLL
                for (int index = 0; index < count; index++)
                {
                    first_color_index = _Positions[index] <= current_position ? index : first_color_index;
                }
                const int second_color_index = min(first_color_index + 1, count - 1);
                const float delta = (current_position - _Positions[first_color_index]) / max(0.001, _Positions[second_color_index] - _Positions[first_color_index]);
                const fixed4 first_color = _Colors[first_color_index];
                const fixed4 second_color = _Colors[second_color_index];

                fixed4 color = lerp(first_color, second_color, delta);

                #ifdef UNITY_COLORSPACE_GAMMA
                return color;
                #endif

                return fixed4(GammaToLinearSpace(color.rgb), color.a);
            }
            ENDCG
        }
    }
}
