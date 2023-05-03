Shader "Hidden/App UI/Box"
{
    Properties
    {
        _Ratio("Ratio", Float) = 1.0
        _Rect("Rect", Vector) = (0,0,1,1)
        _Color("Color", Color) = (1,1,1,1)
        _BackgroundImage("Background Image", 2D) = "black" {}
        _BackgroundImageTransform("Background Image Transform", Vector) = (0,0,1,1)
        _BackgroundSize("Background Size", Int) = 0
        _Radiuses("Radiuses", Vector) = (0,0,0.5,0.25)
        _AA("Anti Aliasing", Float) = 0.0012
        [Header(Border)]
        [Toggle(ENABLE_BORDER)] _EnableBorder("Enable", Float) = 1
        _BorderThickness("Border Thickness", Range(0,1)) = 0.012
        _BorderColor("Border Color", Color) = (1,1,1,1)
        [Header(Shadow)]
        [Toggle(ENABLE_SHADOW)] _EnableShadow("Enable", Float) = 1
        _ShadowOffset("Shadow Offset", Vector) = (0.1, 0.1, 0.02, 0.005)
        _ShadowColor("Shadow Color", Color) = (0,0,0,1)
        _ShadowInset("Shadow Inset Mode", Int) = 0
        [Header(Outline)]
        [Toggle(ENABLE_OUTLINE)] _EnableOutline("Enable", Float) = 1
        _OutlineThickness("Outline Thickness", Range(0,1)) = 0.012
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineOffset("Outline Offset", Range(0,1)) = 0.0
    }
    SubShader
    {
        CGINCLUDE

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
        float4 _Rect;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = float2((v.uv.x - _Rect.x) * 1.0, ((1.0 - v.uv.y - _Rect.y) * 1.0) * _Ratio); // reverse the Y axis because UITK uses origin at top-left corner
            return o;
        }

        float4 _Radiuses;
        float _AA;

        ENDCG

        Pass
        {
            Name "Clear"
            ColorMask RGBA
            Color (0,0,0,0)
        }

        Pass
        {
            Name "BoxShadows"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ ENABLE_SHADOW

            fixed4 _ShadowColor;
            float4 _ShadowOffset;
            int _ShadowInset;

            fixed4 frag (v2f i) : SV_Target
            {
                #if ENABLE_SHADOW
                if (_ShadowInset == 1)
                    return fixed4(0,0,0,0);
                // Find clip box mask
                float2 boxClipSSize = _Rect.zw + _AA;
                const float2 halfSize = float2(boxClipSSize.x, boxClipSSize.y * _Ratio) * 0.5;
                const float sd = roundedBoxSDF(i.uv, halfSize, _Radiuses.zywx);
                // Find rounded shadow box mask
                float radius = (_Radiuses.x + _Radiuses.y + _Radiuses.z + _Radiuses.w) * 0.25;
                float shadowWidth = _Rect.z + _ShadowOffset.z * 2.0;
                float shadowHeight = _Rect.w + _ShadowOffset.z * 2.0;
                radius *= min(shadowWidth / _Rect.z, shadowHeight / _Rect.w);
                radius = min(min(shadowWidth, shadowHeight) * 0.5, radius);
                float2 lower = (-_Rect.zw * 0.5) + _ShadowOffset.xy - _ShadowOffset.zz;
                float2 upper = lower + float2(shadowWidth, shadowHeight);
                lower.y *= _Ratio;
                upper.y *= _Ratio;
                const float sigma = max(0.0001, _ShadowOffset.w);
                float shadow = roundedBoxShadow(lower, upper, i.uv, sigma, radius);
                // apply shadow box mask
                shadow *= boxShadow(lower, upper, i.uv, sigma);
                // apply clip box mask based on Inset value
                const float outsetShadow = shadow * max(float(sd > 0.), 0.);
                return fixed4(_ShadowColor.rgb, outsetShadow * _ShadowColor.a);
                #else
                return fixed4(0,0,0,0);
                #endif
            }
            ENDCG
        }

        Pass
        {
            Name "BackgroundColor"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _BorderThickness;
            fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                const float2 rectSize = _Rect.zw;
                const float2 halfSize = float2(rectSize.x, rectSize.y * _Ratio) * 0.5;
                const float distanceWithoutBorder = roundedBoxSDF(i.uv, halfSize, _Radiuses.zywx);
                const float smoothedAlphaWithoutBorder = 1.0 - smoothstep(0.0, _AA, distanceWithoutBorder);
                return fixed4(_Color.rgb, _Color.a * smoothedAlphaWithoutBorder);
            }
            ENDCG
        }

        Pass
        {
            Name "BackgroundImage"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _BorderThickness;
            sampler2D _BackgroundImage;
            float4 _BackgroundImageTransform;
            int _BackgroundSize;

            fixed4 frag (v2f i) : SV_Target
            {
                const float2 rectSize = _Rect.zw - _BorderThickness;
                const float2 halfSize = float2(rectSize.x, rectSize.y * _Ratio) * 0.5;
                const float distanceWithoutBorder = roundedBoxSDF(i.uv, halfSize, _Radiuses.zywx);
                const float smoothedAlphaWithoutBorder = 1.0 - smoothstep(0.0, _AA, distanceWithoutBorder);

                const float xMin = - _Rect.z * 0.5 + _BorderThickness;
                const float xMax = _Rect.z * 0.5 - _BorderThickness;
                const float yMin = - _Rect.w * 0.5 + _BorderThickness;
                const float yMax = _Rect.w * 0.5 - _BorderThickness;

                const float imgWidth = _BackgroundImageTransform.z;
                const float imgHeight = _BackgroundImageTransform.w;

                const float containerHeight = abs(yMax - yMin);
                const float containerWidth = abs(xMax - xMin);

                const float wRatio = containerWidth / imgWidth;
                const float hRatio = (containerHeight / imgHeight) * _Ratio;

                // background-size (cover, contain, stretch)
                float2 coverRatio = float2(max(wRatio, hRatio), max(wRatio, hRatio));
                float2 containRatio = float2(min(wRatio, hRatio), min(wRatio, hRatio));
                float2 stretch = float2(wRatio, hRatio);

                float2 mode = _BackgroundSize == 0 ? coverRatio : _BackgroundSize == 1 ? containRatio : stretch;
                float2 uvScale = float2(imgWidth, imgHeight) * mode;

                const float2 texcoord = (float2(i.uv.x, -i.uv.y) / uvScale) + 0.5;

                // background-repeat (no-repeat)
                clip(texcoord.x < 0 || texcoord.x > 1 || texcoord.y < 0 || texcoord.y > 1 ? -1 : 1);

                const fixed4 img = tex2D(_BackgroundImage, texcoord);
                return fixed4(img.rgb, img.a * smoothedAlphaWithoutBorder);
            }
            ENDCG
        }

        Pass
        {
            Name "InsetBoxShadows"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ ENABLE_SHADOW

            fixed4 _ShadowColor;
            float4 _ShadowOffset;
            int _ShadowInset;

            fixed4 frag (v2f i) : SV_Target
            {
                #if ENABLE_SHADOW
                if (_ShadowInset == 0)
                    return fixed4(0,0,0,0);
                // Find clip box mask
                float2 boxClipSSize = _Rect.zw;
                const float2 halfSize = float2(boxClipSSize.x, boxClipSSize.y * _Ratio) * 0.5;
                const float sd = roundedBoxSDF(i.uv, halfSize, _Radiuses.zywx);
                // Find rounded shadow box mask
                float radius = (_Radiuses.x + _Radiuses.y + _Radiuses.z + _Radiuses.w) * 0.25;
                float shadowWidth = _Rect.z + _ShadowOffset.z * 2.0;
                float shadowHeight = _Rect.w + _ShadowOffset.z * 2.0;
                radius *= min(shadowWidth / _Rect.z, shadowHeight / _Rect.w);
                radius = min(min(shadowWidth, shadowHeight) * 0.5, radius);
                float2 lower = (-_Rect.zw * 0.5) + _ShadowOffset.xy - _ShadowOffset.zz;
                float2 upper = lower + float2(shadowWidth, shadowHeight);
                lower.y *= _Ratio;
                upper.y *= _Ratio;
                const float sigma = max(0.0001, _ShadowOffset.w);
                float shadow = roundedBoxShadow(lower, upper, i.uv, sigma, radius);
                // apply shadow box mask
                shadow *= boxShadow(lower, upper, i.uv, sigma);
                // apply clip box mask based on Inset value
                const float insetShadow = (1.0 - shadow) * max(float(sd < 0.), 0.);
                return fixed4(_ShadowColor.rgb, insetShadow * _ShadowColor.a);
                #else
                return fixed4(0,0,0,0);
                #endif
            }
            ENDCG
        }

        Pass
        {
            Name "Border"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ ENABLE_BORDER

            float _BorderThickness;
            fixed4 _BorderColor;

            fixed4 frag (v2f i) : SV_Target
            {
                #if ENABLE_BORDER
                const float2 halfSize = float2(_Rect.z, _Rect.w * _Ratio) * 0.5;
                const float distance = roundedBoxSDF(i.uv, halfSize, _Radiuses.zywx);
                const float smoothedAlpha = 1.0 - smoothstep(0.0, _AA, distance);
                const float borderAlpha = 1.0 - smoothstep(_BorderThickness - _AA, _BorderThickness, abs(distance));
                return fixed4(_BorderColor.rgb, _BorderColor.a * min(borderAlpha, smoothedAlpha));
                #else
                return fixed4(0,0,0,0);
                #endif
            }
            ENDCG
        }

        Pass
        {
            Name "Outline"
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ ENABLE_OUTLINE

            float _OutlineThickness;
            float _OutlineOffset;
            fixed4 _OutlineColor;

            fixed4 frag (v2f i) : SV_Target
            {
                #if ENABLE_OUTLINE
                const float2 halfSize = float2(_Rect.z, _Rect.w * _Ratio) * 0.5;
                const float distance = roundedBoxSDF(i.uv, halfSize, _Radiuses.zywx);
                const float outerAlpha = 1.0 - smoothstep(_OutlineOffset, _OutlineOffset + _AA, distance);
                const float innerAlpha = 1.0 - smoothstep(_OutlineOffset - _OutlineThickness + _AA * 0.5, _OutlineOffset - _OutlineThickness, distance);
                return fixed4(_OutlineColor.rgb, _OutlineColor.a * min(innerAlpha, outerAlpha));
                #else
                return fixed4(0,0,0,0);
                #endif
            }
            ENDCG
        }
    }
}
