using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A base class for all progress UI elements. This class is not meant to be used directly.
    /// </summary>
    public abstract class Progress : VisualElement, ISizeableElement
    {
        const int k_MessageUpdateProgressAnim = 93;
        
        static readonly Vertex[] k_Vertices = new Vertex[4];
        static readonly ushort[] k_Indices = { 0, 1, 2, 2, 3, 0 };
        
        static Progress()
        {
            k_Vertices[0].tint = Color.white;
            k_Vertices[1].tint = Color.white;
            k_Vertices[2].tint = Color.white;
            k_Vertices[3].tint = Color.white;
        }

        /// <summary>
        /// The progress variant.
        /// </summary>
        public enum Variant
        {
            /// <summary>
            /// The progress is indeterminate. A loop animation is displayed.
            /// </summary>
            Indeterminate = 0,
            /// <summary>
            /// The progress is determinate. The real progress is displayed.
            /// </summary>
            Determinate,
        }

        static Material s_Material;

        /// <summary>
        /// The Progress main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-progress";

        /// <summary>
        /// The Progress image styling class.
        /// </summary>
        public static readonly string imageUssClassName = ussClassName + "__image";

        /// <summary>
        /// The Progress size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Progress variant styling class.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";

        static readonly CustomStyleProperty<Color> k_UssColor = new CustomStyleProperty<Color>("--progress-color");

        Size m_Size;

        Color? m_ColorOverride;

        Variant m_Variant;

        float m_Value;

        float m_BufferValue;

        /// <summary>
        /// The image that contains the rendered texture of the progress.
        /// </summary>
        protected readonly VisualElement m_Image;

        /// <summary>
        /// The rendered texture of the progress.
        /// </summary>
        protected RenderTexture m_RT;

        Vector2 m_PreviousSize;

        /// <summary>
        /// The main color of the progress.
        /// </summary>
        protected Color m_Color;

        float m_BufferOpacity = 0.1f;

        IVisualElementScheduledItem m_Update;

        /// <summary>
        /// The content container of the progress.
        /// </summary>
        public override VisualElement contentContainer => m_Image.contentContainer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected Progress()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            m_Image = new Image { name = imageUssClassName, pickingMode = PickingMode.Ignore };
            m_Image.AddToClassList(imageUssClassName);
            hierarchy.Add(m_Image);

            m_Color = new Color(0.82f, 0.82f, 0.82f);
            variant = Variant.Indeterminate;
            size = Size.M;
            value = 0;
            bufferValue = 0;

            m_Image.generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
        }
        
        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                m_Update?.Pause();
                m_Update = null;
                m_Update = schedule.Execute(MarkContentDirtyRepaint).Every(8L);
            }
        }

        void MarkContentDirtyRepaint()
        {
            m_Image.MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            GenerateTextures();
            
            var left = paddingRect.xMin;
            var right = paddingRect.xMax;
            var top = paddingRect.yMin;
            var bottom = paddingRect.yMax;

            k_Vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
            k_Vertices[1].position = new Vector3(left, top, Vertex.nearZ);
            k_Vertices[2].position = new Vector3(right, top, Vertex.nearZ);
            k_Vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);
            
            var mwd = mgc.Allocate(k_Vertices.Length, k_Indices.Length, m_RT);

#if !UNITY_2023_1_OR_NEWER
            // Since the texture may be stored in an atlas, the UV coordinates need to be
            // adjusted. Simply rescale them in the provided uvRegion.
            var uvRegion = mwd.uvRegion;
#else
            var uvRegion = new Rect(0, 0, 1, 1);
#endif
            k_Vertices[0].uv = new Vector2(0, 0) * uvRegion.size + uvRegion.min;
            k_Vertices[1].uv = new Vector2(0, 1) * uvRegion.size + uvRegion.min;
            k_Vertices[2].uv = new Vector2(1, 1) * uvRegion.size + uvRegion.min;
            k_Vertices[3].uv = new Vector2(1, 0) * uvRegion.size + uvRegion.min;

            mwd.SetAllVertices(k_Vertices);
            mwd.SetAllIndices(k_Indices);
        }

        /// <summary>
        /// The LinearProgress size.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
            }
        }

        /// <summary>
        /// The variant of the progress.
        /// </summary>
        public Variant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(variantUssClassName + m_Variant.ToString().ToLower());
            }
        }

        /// <summary>
        /// The opacity of the secondary progress (buffer).
        /// </summary>
        public float bufferOpacity
        {
            get => m_BufferOpacity;
            set
            {
                m_BufferOpacity = value;
                MarkContentDirtyRepaint();
            }
        }

        /// <summary>
        /// The color of the progress.
        /// </summary>
        public Color? colorOverride
        {
            get => m_ColorOverride;
            set
            {
                m_ColorOverride = value;
                MarkContentDirtyRepaint();
            }
        }

        /// <summary>
        /// The progress value.
        /// </summary>
        public float value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                MarkContentDirtyRepaint();
            }
        }

        /// <summary>
        /// The secondary progress (buffer) value.
        /// </summary>
        public float bufferValue
        {
            get => m_BufferValue;
            set
            {
                m_BufferValue = value;
                MarkContentDirtyRepaint();
            }
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (m_RT)
                m_RT.Release();

            m_RT = null;
            
            m_Update?.Pause();
            m_Update = null;
        }

        void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(k_UssColor, out var c))
            {
                m_Color = c;
            }

            MarkContentDirtyRepaint();
        }

        /// <summary>
        /// Generates the textures for the progress.
        /// </summary>
        protected virtual void GenerateTextures() { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Progress"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlColorAttributeDescription m_Color = new UxmlColorAttributeDescription
            {
                name = "color",
                defaultValue = Color.white,
            };

            readonly UxmlFloatAttributeDescription m_Value = new UxmlFloatAttributeDescription
            {
                name = "value",
                defaultValue = 0,
            };

            readonly UxmlFloatAttributeDescription m_ValueBuffer = new UxmlFloatAttributeDescription
            {
                name = "buffer-value",
                defaultValue = 0,
            };

            readonly UxmlFloatAttributeDescription m_BufferOpacity = new UxmlFloatAttributeDescription
            {
                name = "buffer-opacity",
                defaultValue = 0.1f,
            };

            readonly UxmlEnumAttributeDescription<Variant> m_Variant = new UxmlEnumAttributeDescription<Variant>
            {
                name = "variant",
                defaultValue = Variant.Indeterminate,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);

                var element = (Progress)ve;
                element.variant = m_Variant.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.value = m_Value.GetValueFromBag(bag, cc);
                element.bufferValue = m_ValueBuffer.GetValueFromBag(bag, cc);
                element.bufferOpacity = m_BufferOpacity.GetValueFromBag(bag, cc);
                var color = Color.white;
                if (m_Color.TryGetValueFromBag(bag, cc, ref color))
                    element.colorOverride = color;
            }
        }
    }
}
