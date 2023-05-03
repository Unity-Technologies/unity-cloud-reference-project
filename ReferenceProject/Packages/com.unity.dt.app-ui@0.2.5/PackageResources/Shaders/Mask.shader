Shader "Hidden/App UI/Mask"
{
    Properties
    {
        _InnerMaskColor ("Inner Mask Color", Color) = (0,0,0,1)
        _OuterMaskColor ("Outer Mask Color", Color) = (0,0,0,0)
        _MaskRect ("Mask Rect", Vector) = (0.25,0.25,0.5,0.5)
        _Radius ("Radius", Float) = 0.01
        _Ratio ("Ratio", Float) = 1.77
        _Sigma ("Sigma", Float) = 0.001
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

            float _Ratio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = float2(v.uv.x, (1.0 - v.uv.y) * (1.0 / _Ratio) );
                return o;
            }

            float4 _MaskRect;
            fixed4 _InnerMaskColor;
            fixed4 _OuterMaskColor;
            float _Radius;
            float _Sigma;

            fixed4 frag (v2f i) : SV_Target
            {
                const float radius = min(min(_MaskRect.z, _MaskRect.w) * 0.5, _Radius);
                const float2 lower = _MaskRect.xy;
                const float2 upper = _MaskRect.xy + _MaskRect.zw;
                const float sigma = max(0.0001, _Sigma);
                const float shadow = roundedBoxShadow(lower, upper, i.uv, sigma, radius);
                return lerp(_OuterMaskColor, _InnerMaskColor, shadow);
            }
            ENDCG
        }
    }
}
