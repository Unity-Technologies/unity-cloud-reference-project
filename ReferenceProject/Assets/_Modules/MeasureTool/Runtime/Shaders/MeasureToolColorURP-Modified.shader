// This is the Auto-Generated shader for MeasureToolColor's shader graph in URP with these modifications:
// 1. Change "ZTest LEqual" to "ZTest Always" for both Pass named "Pass"
// 2. The shader name was changed to "UnityReflect/MeasureToolColorURP-Modified"
// 3. Adding the PackageRequirements section under all SubShader
Shader "UnityReflect/MeasureToolColorURP-Modified"
{
    Properties
    {
        _MeasureTool_Color("Color", Color) = (0.7882353, 0.4235294, 0, 1)
        _MeasureTool_HiddenAlpha("HiddenAlpha", Float) = 0.51
        _MeasureTool_DepthAtFullAlpha("DepthFullAlpha", Float) = 0.03
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
            #define VARYINGS_NEED_POSITION_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
        float4 _MeasureTool_Color;
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions

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
            float4 _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0 = _MeasureTool_Color;
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[3];
            float4 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4;
            float3 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            float2 _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6;
            Unity_Combine_float(_Split_7e78be2a6dfe440fa501cde1461dc119_R_1, _Split_7e78be2a6dfe440fa501cde1461dc119_G_2, _Split_7e78be2a6dfe440fa501cde1461dc119_B_3, 0, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5, _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6);
            float _Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b;
            _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0, _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1);
            float _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1, _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2);
            surface.BaseColor = _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            surface.Alpha = _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
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
            #define VARYINGS_NEED_POSITION_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
        float4 _MeasureTool_Color;
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions

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
            float4 _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0 = _MeasureTool_Color;
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[3];
            float _Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b;
            _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0, _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1);
            float _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1, _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2);
            surface.Alpha = _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
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
            #define VARYINGS_NEED_POSITION_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
        float4 _MeasureTool_Color;
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions

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
            float4 _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0 = _MeasureTool_Color;
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[3];
            float _Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b;
            _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0, _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1);
            float _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1, _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2);
            surface.Alpha = _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
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
            #define VARYINGS_NEED_POSITION_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
        float4 _MeasureTool_Color;
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions

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
            float4 _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0 = _MeasureTool_Color;
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[3];
            float4 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4;
            float3 _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            float2 _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6;
            Unity_Combine_float(_Split_7e78be2a6dfe440fa501cde1461dc119_R_1, _Split_7e78be2a6dfe440fa501cde1461dc119_G_2, _Split_7e78be2a6dfe440fa501cde1461dc119_B_3, 0, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGBA_4, _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5, _Combine_cefdd432261c4b80a6b19527a3fcce36_RG_6);
            float _Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b;
            _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0, _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1);
            float _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1, _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2);
            surface.BaseColor = _Combine_cefdd432261c4b80a6b19527a3fcce36_RGB_5;
            surface.Alpha = _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
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
            #define VARYINGS_NEED_POSITION_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
        float4 _MeasureTool_Color;
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions

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
            float4 _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0 = _MeasureTool_Color;
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[3];
            float _Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b;
            _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0, _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1);
            float _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1, _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2);
            surface.Alpha = _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
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
            #define VARYINGS_NEED_POSITION_WS
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
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS;
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
        float4 _MeasureTool_Color;
        float _MeasureTool_HiddenAlpha;
        float _MeasureTool_DepthAtFullAlpha;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions

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
            float4 _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0 = _MeasureTool_Color;
            float _Split_7e78be2a6dfe440fa501cde1461dc119_R_1 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[0];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_G_2 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[1];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_B_3 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[2];
            float _Split_7e78be2a6dfe440fa501cde1461dc119_A_4 = _Property_5ab61c432f7b40c4970352d7cee905a3_Out_0[3];
            float _Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0 = _MeasureTool_DepthAtFullAlpha;
            float _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0 = _MeasureTool_HiddenAlpha;
            Bindings_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9 _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b;
            _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b.ScreenPosition = IN.ScreenPosition;
            float _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1;
            SG_MeasureToolAlpha_8027056915d927f41826bb02d02c99b9(_Property_c6a4aeff4f224b39aa4d5539c48cdf38_Out_0, _Property_1904f5349b284b4bbfc3b91dbe9998dd_Out_0, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1);
            float _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
            Unity_Multiply_float(_Split_7e78be2a6dfe440fa501cde1461dc119_A_4, _MeasureToolAlpha_058967342bed41aab3ba2d551876ae6b_OutVector1_1, _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2);
            surface.Alpha = _Multiply_3ed0fd94c7f149ef9f67d104e07ea06a_Out_2;
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
