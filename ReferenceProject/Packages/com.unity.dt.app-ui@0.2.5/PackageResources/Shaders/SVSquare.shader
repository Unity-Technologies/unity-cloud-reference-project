Shader "Hidden/App UI/SVSquare"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
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
                o.uv = v.uv;
                return o;
            }

            fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                const half4 saturation = lerp(half4(1,1,1,1), half4(_Color.rgb, 1), i.uv.x);
                const float brightness = i.uv.y;
                const fixed4 color = fixed4(saturation.rgb * brightness, 1.);

                #ifdef UNITY_COLORSPACE_GAMMA
                return color;
                #endif

                return fixed4(GammaToLinearSpace(color.rgb), color.a);
            }
            ENDCG
        }
    }
}
