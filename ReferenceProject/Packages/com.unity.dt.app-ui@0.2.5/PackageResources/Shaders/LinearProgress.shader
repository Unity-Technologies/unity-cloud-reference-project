Shader "Hidden/App UI/LinearProgress"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Float) = 0
        _Start ("Start", Float) = 0
        _End ("End", Float) = 0
        _BufferStart ("Buffer Start", Float) = 0
        _BufferEnd ("Buffer End", Float) = 0
        _BufferOpacity ("Buffer Opacity", Float) = 0.1
        _AA ("Anti-Aliasing", Float) = 0.005
        _Phase("Phase", Vector) = (0,0,0,0)
        _Ratio ("Ratio", Float) = 1.0
        _Padding ("Padding", Float) = 1.0
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
            #pragma multi_compile_local __ PROGRESS_INDETERMINATE

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

            float _Ratio;
            float _Padding;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = float2(v.uv.x / (1.0 - _Padding * 2.0) - _Padding, (v.uv.y - 0.5) / _Ratio);
                return o;
            }

            float _InnerRadius;
            float _Start;
            float _End;
            float _BufferStart;
            float _BufferEnd;
            float _BufferOpacity;
            fixed4 _Color;
            float _AA;
            float4 _Phase;

            inline float circle(float2 uv, float2 pos, float rad)
            {
                return 1.0 - smoothstep(rad, rad + _AA, length(uv-pos));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                #if PROGRESS_INDETERMINATE
                const float duration = 1.0;
                const float time = fmod(_Phase.y / duration, 1.0);
                #endif
                const float progress = i.uv.x;

                #if PROGRESS_INDETERMINATE
                _Start = time < 0.5 ? lerp(0.0, 0.15, time / 0.5) : time < 0.75 ? lerp(0.15, 0.2, (time - 0.5) / 0.25) : lerp(0.2, 0.99, (time - 0.75) / 0.25);
                _End = time < 0.5 ? lerp(0.0, 0.8, time / 0.5) : time < 0.65 ? lerp(0.8, 0.85, (time - 0.5) / 0.15) : time < 0.8 ? lerp(0.85, 1.0, (time - 0.65) / 0.15) : 1.0;
                #endif

                // Mask for the value progress
                const float radius = 1.0 / _Ratio * 0.5;
                float valueMask = progress >= _Start && progress <= _End ? 1.0 : 0.0;
                const float startCircle = circle(i.uv, float2(_Start, 0), radius);
                const float endCircle = circle(i.uv, float2(_End, 0), radius);
                valueMask = max(valueMask, startCircle);
                valueMask = max(valueMask, endCircle);

                fixed4 color = fixed4(_Color.rgb, 1);
                color.a *= valueMask;

                #ifndef PROGRESS_INDETERMINATE
                // Mask for the buffer progress
                float bufferMask = progress >= _BufferStart && progress <= _BufferEnd ? 1.0 : 0.0;
                const float startBufferCircle = circle(i.uv, float2(_BufferStart, 0), radius);
                const float endBufferCircle = circle(i.uv, float2(_BufferEnd, 0), radius);
                bufferMask = max(bufferMask, startBufferCircle);
                bufferMask = max(bufferMask, endBufferCircle);

                color.a = max(color.a, _BufferEnd > 0 ? _BufferOpacity * bufferMask : _BufferOpacity);

                #else
                // Make the rest of the bar visible but with low opacity
                color.a = max(color.a, _BufferOpacity);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
