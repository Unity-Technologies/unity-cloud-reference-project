// This is the Auto-Generated shader for MeasureToolTexture's shader graph in URP with these modifications:
// 1. Change "ZTest LEqual" to "ZTest Always" for both Pass named "Pass"
// 2. The shader name was changed to "UnityReflect/MeasureToolTextureURP-Modified"
// 3. Adding the PackageRequirements section under all SubShader
Shader "UnityReflect/MeasureToolTextureURP-Modified"
{
    Properties
    {
        _MeasureTool_HiddenAlpha("HiddenAlpha", Float) = 0.51
        _MeasureTool_DepthAtFullAlpha("DepthFullAlpha", Float) = 0.03
        [NoScaleOffset]_MeasureTool_Texture("Texture", 2D) = "white" {}
        _MeasureTool_TextureTiling("TexutreTiling", Vector) = (1, 1, 0, 0)
        _MeasureTool_Color("Color", Color) = (1, 1, 1, 1)
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest Always
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        float4 _MeasureTool_Texture_TexelSize;
        float2 _MeasureTool_TextureTiling;
        float4 _MeasureTool_Color;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MeasureTool_Texture);
        SAMPLER(sampler_MeasureTool_Texture);

            // Graph Functions

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

        struct Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9
        {
            float4 ScreenPosition;
        };

        void SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(float Vector1_9360967f9ef94b629da04e394d549bc8, float Vector1_c955b432be6c49af9e3759fb3fe848d2, Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 IN, out float OutVector1_1)
        {
            float _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0 = Vector1_c955b432be6c49af9e3759fb3fe848d2;
            float _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0 = Vector1_9360967f9ef94b629da04e394d549bc8;
            float _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1);
            float4 _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0 = IN.ScreenPosition;
            float _Split_c2772f6af09e45e98495671a924ac870_R_1 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[0];
            float _Split_c2772f6af09e45e98495671a924ac870_G_2 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[1];
            float _Split_c2772f6af09e45e98495671a924ac870_B_3 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[2];
            float _Split_c2772f6af09e45e98495671a924ac870_A_4 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[3];
            float _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2;
            Unity_Subtract_float(_SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1, _Split_c2772f6af09e45e98495671a924ac870_A_4, _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2);
            float _Negate_92e7c006951b46deb99f5167997eda5d_Out_1;
            Unity_Negate_float(_Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1);
            float _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3;
            Unity_Smoothstep_float(0, _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3);
            float _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
            Unity_Lerp_float(1, _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3, _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3);
            OutVector1_1 = _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_b1d30e9e447b4585940821829e917d0a_Out_0 = _MeasureTool_Color;
            UnityTexture2D _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0 = UnityBuildTexture2DStructNoScale(_MeasureTool_Texture);
            float2 _Property_5a0ab326745f42db9c459fa28e11188f_Out_0 = _MeasureTool_TextureTiling;
            float2 _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_5a0ab326745f42db9c459fa28e11188f_Out_0, float2 (0, 0), _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float4 _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0 = SAMPLE_TEXTURE2D(_Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.tex, _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.samplerstate, _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_R_4 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.r;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_G_5 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.g;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_B_6 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.b;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_A_7 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.a;
            float4 _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2;
            Unity_Multiply_float(_Property_b1d30e9e447b4585940821829e917d0a_Out_0, _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0, _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2);
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[3];
            float4 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4;
            float3 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            float2 _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6;
            Unity_Combine_float(_Split_7e78be2a6dfe440fa501cde1461dc119_R_1, _Split_7e78be2a6dfe440fa501cde1461dc119_G_2, _Split_7e78be2a6dfe440fa501cde1461dc119_B_3, 0, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5, _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6);
            float _Property_edd100db78a445329b7594825346da31_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_583172ff22a640e09a76dcda28c66e56_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e;
            _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_edd100db78a445329b7594825346da31_Out_0, _Property_583172ff22a640e09a76dcda28c66e56_Out_0, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1);
            float _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1, _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2);
            surface.BaseColor = _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            surface.Alpha = _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        float4 _MeasureTool_Texture_TexelSize;
        float2 _MeasureTool_TextureTiling;
        float4 _MeasureTool_Color;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MeasureTool_Texture);
        SAMPLER(sampler_MeasureTool_Texture);

            // Graph Functions

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

        struct Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9
        {
            float4 ScreenPosition;
        };

        void SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(float Vector1_9360967f9ef94b629da04e394d549bc8, float Vector1_c955b432be6c49af9e3759fb3fe848d2, Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 IN, out float OutVector1_1)
        {
            float _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0 = Vector1_c955b432be6c49af9e3759fb3fe848d2;
            float _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0 = Vector1_9360967f9ef94b629da04e394d549bc8;
            float _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1);
            float4 _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0 = IN.ScreenPosition;
            float _Split_c2772f6af09e45e98495671a924ac870_R_1 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[0];
            float _Split_c2772f6af09e45e98495671a924ac870_G_2 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[1];
            float _Split_c2772f6af09e45e98495671a924ac870_B_3 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[2];
            float _Split_c2772f6af09e45e98495671a924ac870_A_4 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[3];
            float _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2;
            Unity_Subtract_float(_SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1, _Split_c2772f6af09e45e98495671a924ac870_A_4, _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2);
            float _Negate_92e7c006951b46deb99f5167997eda5d_Out_1;
            Unity_Negate_float(_Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1);
            float _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3;
            Unity_Smoothstep_float(0, _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3);
            float _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
            Unity_Lerp_float(1, _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3, _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3);
            OutVector1_1 = _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_b1d30e9e447b4585940821829e917d0a_Out_0 = _MeasureTool_Color;
            UnityTexture2D _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0 = UnityBuildTexture2DStructNoScale(_MeasureTool_Texture);
            float2 _Property_5a0ab326745f42db9c459fa28e11188f_Out_0 = _MeasureTool_TextureTiling;
            float2 _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_5a0ab326745f42db9c459fa28e11188f_Out_0, float2 (0, 0), _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float4 _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0 = SAMPLE_TEXTURE2D(_Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.tex, _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.samplerstate, _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_R_4 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.r;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_G_5 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.g;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_B_6 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.b;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_A_7 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.a;
            float4 _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2;
            Unity_Multiply_float(_Property_b1d30e9e447b4585940821829e917d0a_Out_0, _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0, _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2);
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[3];
            float _Property_edd100db78a445329b7594825346da31_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_583172ff22a640e09a76dcda28c66e56_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e;
            _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_edd100db78a445329b7594825346da31_Out_0, _Property_583172ff22a640e09a76dcda28c66e56_Out_0, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1);
            float _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1, _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2);
            surface.Alpha = _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        float4 _MeasureTool_Texture_TexelSize;
        float2 _MeasureTool_TextureTiling;
        float4 _MeasureTool_Color;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MeasureTool_Texture);
        SAMPLER(sampler_MeasureTool_Texture);

            // Graph Functions

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

        struct Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9
        {
            float4 ScreenPosition;
        };

        void SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(float Vector1_9360967f9ef94b629da04e394d549bc8, float Vector1_c955b432be6c49af9e3759fb3fe848d2, Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 IN, out float OutVector1_1)
        {
            float _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0 = Vector1_c955b432be6c49af9e3759fb3fe848d2;
            float _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0 = Vector1_9360967f9ef94b629da04e394d549bc8;
            float _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1);
            float4 _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0 = IN.ScreenPosition;
            float _Split_c2772f6af09e45e98495671a924ac870_R_1 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[0];
            float _Split_c2772f6af09e45e98495671a924ac870_G_2 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[1];
            float _Split_c2772f6af09e45e98495671a924ac870_B_3 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[2];
            float _Split_c2772f6af09e45e98495671a924ac870_A_4 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[3];
            float _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2;
            Unity_Subtract_float(_SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1, _Split_c2772f6af09e45e98495671a924ac870_A_4, _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2);
            float _Negate_92e7c006951b46deb99f5167997eda5d_Out_1;
            Unity_Negate_float(_Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1);
            float _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3;
            Unity_Smoothstep_float(0, _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3);
            float _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
            Unity_Lerp_float(1, _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3, _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3);
            OutVector1_1 = _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_b1d30e9e447b4585940821829e917d0a_Out_0 = _MeasureTool_Color;
            UnityTexture2D _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0 = UnityBuildTexture2DStructNoScale(_MeasureTool_Texture);
            float2 _Property_5a0ab326745f42db9c459fa28e11188f_Out_0 = _MeasureTool_TextureTiling;
            float2 _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_5a0ab326745f42db9c459fa28e11188f_Out_0, float2 (0, 0), _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float4 _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0 = SAMPLE_TEXTURE2D(_Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.tex, _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.samplerstate, _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_R_4 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.r;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_G_5 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.g;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_B_6 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.b;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_A_7 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.a;
            float4 _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2;
            Unity_Multiply_float(_Property_b1d30e9e447b4585940821829e917d0a_Out_0, _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0, _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2);
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[3];
            float _Property_edd100db78a445329b7594825346da31_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_583172ff22a640e09a76dcda28c66e56_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e;
            _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_edd100db78a445329b7594825346da31_Out_0, _Property_583172ff22a640e09a76dcda28c66e56_Out_0, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1);
            float _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1, _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2);
            surface.Alpha = _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest Always
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        float4 _MeasureTool_Texture_TexelSize;
        float2 _MeasureTool_TextureTiling;
        float4 _MeasureTool_Color;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MeasureTool_Texture);
        SAMPLER(sampler_MeasureTool_Texture);

            // Graph Functions

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

        struct Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9
        {
            float4 ScreenPosition;
        };

        void SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(float Vector1_9360967f9ef94b629da04e394d549bc8, float Vector1_c955b432be6c49af9e3759fb3fe848d2, Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 IN, out float OutVector1_1)
        {
            float _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0 = Vector1_c955b432be6c49af9e3759fb3fe848d2;
            float _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0 = Vector1_9360967f9ef94b629da04e394d549bc8;
            float _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1);
            float4 _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0 = IN.ScreenPosition;
            float _Split_c2772f6af09e45e98495671a924ac870_R_1 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[0];
            float _Split_c2772f6af09e45e98495671a924ac870_G_2 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[1];
            float _Split_c2772f6af09e45e98495671a924ac870_B_3 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[2];
            float _Split_c2772f6af09e45e98495671a924ac870_A_4 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[3];
            float _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2;
            Unity_Subtract_float(_SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1, _Split_c2772f6af09e45e98495671a924ac870_A_4, _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2);
            float _Negate_92e7c006951b46deb99f5167997eda5d_Out_1;
            Unity_Negate_float(_Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1);
            float _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3;
            Unity_Smoothstep_float(0, _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3);
            float _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
            Unity_Lerp_float(1, _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3, _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3);
            OutVector1_1 = _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_b1d30e9e447b4585940821829e917d0a_Out_0 = _MeasureTool_Color;
            UnityTexture2D _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0 = UnityBuildTexture2DStructNoScale(_MeasureTool_Texture);
            float2 _Property_5a0ab326745f42db9c459fa28e11188f_Out_0 = _MeasureTool_TextureTiling;
            float2 _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_5a0ab326745f42db9c459fa28e11188f_Out_0, float2 (0, 0), _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float4 _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0 = SAMPLE_TEXTURE2D(_Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.tex, _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.samplerstate, _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_R_4 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.r;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_G_5 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.g;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_B_6 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.b;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_A_7 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.a;
            float4 _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2;
            Unity_Multiply_float(_Property_b1d30e9e447b4585940821829e917d0a_Out_0, _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0, _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2);
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[3];
            float4 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4;
            float3 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            float2 _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6;
            Unity_Combine_float(_Split_7e78be2a6dfe440fa501cde1461dc119_R_1, _Split_7e78be2a6dfe440fa501cde1461dc119_G_2, _Split_7e78be2a6dfe440fa501cde1461dc119_B_3, 0, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5, _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6);
            float _Property_edd100db78a445329b7594825346da31_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_583172ff22a640e09a76dcda28c66e56_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e;
            _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_edd100db78a445329b7594825346da31_Out_0, _Property_583172ff22a640e09a76dcda28c66e56_Out_0, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1);
            float _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1, _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2);
            surface.BaseColor = _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            surface.Alpha = _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        float4 _MeasureTool_Texture_TexelSize;
        float2 _MeasureTool_TextureTiling;
        float4 _MeasureTool_Color;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MeasureTool_Texture);
        SAMPLER(sampler_MeasureTool_Texture);

            // Graph Functions

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

        struct Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9
        {
            float4 ScreenPosition;
        };

        void SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(float Vector1_9360967f9ef94b629da04e394d549bc8, float Vector1_c955b432be6c49af9e3759fb3fe848d2, Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 IN, out float OutVector1_1)
        {
            float _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0 = Vector1_c955b432be6c49af9e3759fb3fe848d2;
            float _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0 = Vector1_9360967f9ef94b629da04e394d549bc8;
            float _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1);
            float4 _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0 = IN.ScreenPosition;
            float _Split_c2772f6af09e45e98495671a924ac870_R_1 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[0];
            float _Split_c2772f6af09e45e98495671a924ac870_G_2 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[1];
            float _Split_c2772f6af09e45e98495671a924ac870_B_3 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[2];
            float _Split_c2772f6af09e45e98495671a924ac870_A_4 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[3];
            float _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2;
            Unity_Subtract_float(_SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1, _Split_c2772f6af09e45e98495671a924ac870_A_4, _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2);
            float _Negate_92e7c006951b46deb99f5167997eda5d_Out_1;
            Unity_Negate_float(_Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1);
            float _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3;
            Unity_Smoothstep_float(0, _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3);
            float _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
            Unity_Lerp_float(1, _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3, _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3);
            OutVector1_1 = _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_b1d30e9e447b4585940821829e917d0a_Out_0 = _MeasureTool_Color;
            UnityTexture2D _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0 = UnityBuildTexture2DStructNoScale(_MeasureTool_Texture);
            float2 _Property_5a0ab326745f42db9c459fa28e11188f_Out_0 = _MeasureTool_TextureTiling;
            float2 _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_5a0ab326745f42db9c459fa28e11188f_Out_0, float2 (0, 0), _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float4 _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0 = SAMPLE_TEXTURE2D(_Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.tex, _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.samplerstate, _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_R_4 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.r;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_G_5 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.g;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_B_6 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.b;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_A_7 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.a;
            float4 _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2;
            Unity_Multiply_float(_Property_b1d30e9e447b4585940821829e917d0a_Out_0, _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0, _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2);
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[3];
            float _Property_edd100db78a445329b7594825346da31_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_583172ff22a640e09a76dcda28c66e56_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e;
            _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_edd100db78a445329b7594825346da31_Out_0, _Property_583172ff22a640e09a76dcda28c66e56_Out_0, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1);
            float _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1, _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2);
            surface.Alpha = _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // Render State
            Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
            float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float3 WorldSpacePosition;
            float4 ScreenPosition;
            float4 uv0;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        float4 _MeasureTool_Texture_TexelSize;
        float2 _MeasureTool_TextureTiling;
        float4 _MeasureTool_Color;
        CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MeasureTool_Texture);
        SAMPLER(sampler_MeasureTool_Texture);

            // Graph Functions

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
        }

        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }

        void Unity_Negate_float(float In, out float Out)
        {
            Out = -1 * In;
        }

        void Unity_Smoothstep_float(float Edge1, float Edge2, float In, out float Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }

        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }

        struct Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9
        {
            float4 ScreenPosition;
        };

        void SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(float Vector1_9360967f9ef94b629da04e394d549bc8, float Vector1_c955b432be6c49af9e3759fb3fe848d2, Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 IN, out float OutVector1_1)
        {
            float _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0 = Vector1_c955b432be6c49af9e3759fb3fe848d2;
            float _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0 = Vector1_9360967f9ef94b629da04e394d549bc8;
            float _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1;
            Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1);
            float4 _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0 = IN.ScreenPosition;
            float _Split_c2772f6af09e45e98495671a924ac870_R_1 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[0];
            float _Split_c2772f6af09e45e98495671a924ac870_G_2 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[1];
            float _Split_c2772f6af09e45e98495671a924ac870_B_3 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[2];
            float _Split_c2772f6af09e45e98495671a924ac870_A_4 = _ScreenPosition_638478a20bc541b89d3e2f8d2809361d_Out_0[3];
            float _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2;
            Unity_Subtract_float(_SceneDepth_47502dec66ba4185b7985dd4dc84b22a_Out_1, _Split_c2772f6af09e45e98495671a924ac870_A_4, _Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2);
            float _Negate_92e7c006951b46deb99f5167997eda5d_Out_1;
            Unity_Negate_float(_Subtract_4d44ab5d06864720ad3ec291eb1f3de8_Out_2, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1);
            float _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3;
            Unity_Smoothstep_float(0, _Property_3e4054ac0cb24e31a84edf309a1c5edc_Out_0, _Negate_92e7c006951b46deb99f5167997eda5d_Out_1, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3);
            float _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
            Unity_Lerp_float(1, _Property_5a61ebf8d9084205a2bd64425426f9bd_Out_0, _Smoothstep_5c8cb8ee56d64468a0461d7a18281886_Out_3, _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3);
            OutVector1_1 = _Lerp_6cc6af9bbf324f76a637f2920d71d8f9_Out_3;
        }

        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_b1d30e9e447b4585940821829e917d0a_Out_0 = _MeasureTool_Color;
            UnityTexture2D _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0 = UnityBuildTexture2DStructNoScale(_MeasureTool_Texture);
            float2 _Property_5a0ab326745f42db9c459fa28e11188f_Out_0 = _MeasureTool_TextureTiling;
            float2 _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_5a0ab326745f42db9c459fa28e11188f_Out_0, float2 (0, 0), _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float4 _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0 = SAMPLE_TEXTURE2D(_Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.tex, _Property_4c2a3e9e8c31403fbdfb81bb4a49116f_Out_0.samplerstate, _TilingAndOffset_4255c74050bc4646b75246df69811bd7_Out_3);
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_R_4 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.r;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_G_5 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.g;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_B_6 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.b;
            float _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_A_7 = _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0.a;
            float4 _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2;
            Unity_Multiply_float(_Property_b1d30e9e447b4585940821829e917d0a_Out_0, _SampleTexture2D_9fc92641aa0b4dbaa365a435025736cf_RGBA_0, _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2);
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Multiply_f5d7cdd992484d0e8bcd994ac9f08ddb_Out_2[3];
            float _Property_edd100db78a445329b7594825346da31_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_583172ff22a640e09a76dcda28c66e56_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e;
            _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_edd100db78a445329b7594825346da31_Out_0, _Property_583172ff22a640e09a76dcda28c66e56_Out_0, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1);
            float _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_d2f758659f44496cb6a5475f6e37f04e_OutVector1_1, _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2);
            surface.Alpha = _Multiply_e161410fc9fd44c0b5df44e009111b39_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.WorldSpacePosition =          input.positionWS;
            output.ScreenPosition =              ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                         input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}
