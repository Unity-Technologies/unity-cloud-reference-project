Shader "Custom/Unlit/TransparentColor/Breath"
{
    Properties
    {
        _Color ("Main Color", COLOR) = (1,1,1,1)
        [Toggle(ENABLE_STEREO)] _EnableStereo ("Enable Stereo", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature ENABLE_STEREO
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
#ifdef ENABLE_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed alpha : COLOR0;
#ifdef ENABLE_STEREO
                UNITY_VERTEX_OUTPUT_STEREO
#endif
            };
            
            fixed4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
#ifdef ENABLE_STEREO
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
#endif
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.alpha = lerp(_Color.a * 0.5, _Color.a, cos(_Time.z) + 1);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(_Color.r, _Color.g, _Color.b, i.alpha);
            }
            ENDCG
        }
    }
}
