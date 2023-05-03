using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A visual element that can be used to mask color.
    /// </summary>
    public class Mask : Image
    {
        /// <summary>
        /// The Mask main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-mask";

        /// <summary>
        /// The content container of this element.
        /// </summary>
        public override VisualElement contentContainer => null;

        RenderTexture m_RT;

        static Material s_Material;

        static readonly int k_MaskRect = Shader.PropertyToID("_MaskRect");

        static readonly int k_Radius = Shader.PropertyToID("_Radius");

        static readonly int k_InnerMaskColor = Shader.PropertyToID("_InnerMaskColor");

        static readonly int k_OuterMaskColor = Shader.PropertyToID("_OuterMaskColor");

        static readonly int k_Ratio = Shader.PropertyToID("_Ratio");

        static readonly int k_Sigma = Shader.PropertyToID("_Sigma");

        Vector2 m_PreviousSize;

        Color m_InnerMaskColor = Color.black;

        Color m_OuterMaskColor = Color.clear;

        Rect m_MaskRect = new Rect(100f, 100f, 100f, 40f);

        Rect m_PreviousMaskRect;

        float m_Radius = 0;

        float m_Blur = 0;

        bool m_UseNormalizedMaskRect;

        /// <summary>
        /// The inner mask color. Sets the color of the inner mask.
        /// </summary>
        public Color innerMaskColor
        {
            get => m_InnerMaskColor;
            set
            {
                m_InnerMaskColor = value;
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The outer mask color. The color of the area outside the mask.
        /// </summary>
        public Color outerMaskColor
        {
            get => m_OuterMaskColor;
            set
            {
                m_OuterMaskColor = value;
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The mask rect. Sets the rect of the mask (in pixels or normalized if <see cref="useNormalizedMaskRect"/> is true).
        /// </summary>
        public Rect maskRect
        {
            get => m_MaskRect;
            set
            {
                m_MaskRect = value;
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The mask radius. Sets the radius of the rounded corners (in pixels).
        /// </summary>
        public float radius
        {
            get => m_Radius;
            set
            {
                m_Radius = value;
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The mask blur. Sets the blur of the mask (in pixels).
        /// </summary>
        public float blur
        {
            get => m_Blur;
            set
            {
                m_Blur = value;
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// If true, the mask rect you will provide through <see cref="maskRect"/> must be normalized (0-1) instead of using pixels coordinates.
        /// </summary>
        public bool useNormalizedMaskRect
        {
            get => m_UseNormalizedMaskRect;
            set
            {
                m_UseNormalizedMaskRect = value;
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mask()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var isNullSize =
                paddingRect.width <= Mathf.Epsilon ||
                paddingRect.height <= Mathf.Epsilon;

            var isSameSize =
                Mathf.Approximately(paddingRect.width, m_PreviousSize.x) &&
                Mathf.Approximately(paddingRect.height, m_PreviousSize.y);

            m_PreviousSize.x = paddingRect.width;
            m_PreviousSize.y = paddingRect.height;

            if (!isNullSize && !isSameSize)
            {
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (m_RT)
                RenderTexture.ReleaseTemporary(m_RT);

            m_RT = null;
        }

        void GenerateTextures()
        {
            var rect = paddingRect;

            if (!rect.IsValid())
                return;

            var dpi = Mathf.Max(Platform.mainScreenScale, 1f);
            var texSize = rect.size * dpi;

            if (!texSize.IsValidForTextureSize())
                return;

            if (m_RT && (Mathf.Abs(m_RT.width - texSize.x) > 1 || Mathf.Abs(m_RT.height - texSize.y) > 1))
            {
                RenderTexture.ReleaseTemporary(m_RT);
                m_RT = null;
            }

            if (!m_RT)
            {
                m_RT = RenderTexture.GetTemporary((int)texSize.x, (int)texSize.y, 24);
                m_RT.Create();
            }

            if (!s_Material)
            {
                s_Material = new Material(Shader.Find("Hidden/App UI/Mask")) { hideFlags = HideFlags.HideAndDontSave };
            }

            s_Material.SetColor(k_InnerMaskColor, innerMaskColor);
            s_Material.SetColor(k_OuterMaskColor, outerMaskColor);

            var ratio = rect.width / rect.height;
            var maskRect =
                useNormalizedMaskRect ? new Vector4(m_MaskRect.x, m_MaskRect.y / ratio, m_MaskRect.width, m_MaskRect.height / ratio) :
                new Vector4(m_MaskRect.x / rect.width, (m_MaskRect.y / rect.height) / ratio,
                m_MaskRect.width / rect.width, (m_MaskRect.height / rect.height) / ratio);
            s_Material.SetVector(k_MaskRect, maskRect);
            s_Material.SetFloat(k_Ratio, ratio);
            s_Material.SetFloat(k_Radius, m_Radius / rect.width);
            s_Material.SetFloat(k_Sigma, m_Blur / rect.width);

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;

            if (image != m_RT)
                image = m_RT;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Mask"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Mask, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ExVisualElement"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlColorAttributeDescription m_InnerMaskColor = new UxmlColorAttributeDescription
            {
                name = "inner-mask-color",
                defaultValue = Color.black
            };

            readonly UxmlColorAttributeDescription m_OuterMaskColor = new UxmlColorAttributeDescription
            {
                name = "outer-mask-color",
                defaultValue = Color.clear
            };

            readonly UxmlFloatAttributeDescription m_Radius = new UxmlFloatAttributeDescription
            {
                name = "radius",
                defaultValue = 0
            };

            readonly UxmlFloatAttributeDescription m_Blur = new UxmlFloatAttributeDescription
            {
                name = "blur",
                defaultValue = 0
            };

            readonly UxmlFloatAttributeDescription m_MaskRectX = new UxmlFloatAttributeDescription
            {
                name = "mask-rect-x",
                defaultValue = 0
            };

            readonly UxmlFloatAttributeDescription m_MaskRectY = new UxmlFloatAttributeDescription
            {
                name = "mask-rect-y",
                defaultValue = 0
            };

            readonly UxmlFloatAttributeDescription m_MaskRectWidth = new UxmlFloatAttributeDescription
            {
                name = "mask-rect-width",
                defaultValue = 0
            };

            readonly UxmlFloatAttributeDescription m_MaskRectHeight = new UxmlFloatAttributeDescription
            {
                name = "mask-rect-height",
                defaultValue = 0
            };

            readonly UxmlBoolAttributeDescription m_UseNormalizedMaskRect = new UxmlBoolAttributeDescription
            {
                name = "use-normalized-mask-rect",
                defaultValue = false
            };

            /// <summary>
            /// Initialize the <see cref="Mask"/> using values from the attribute bag.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> to read values from.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var mask = (Mask)ve;
                mask.innerMaskColor = m_InnerMaskColor.GetValueFromBag(bag, cc);
                mask.outerMaskColor = m_OuterMaskColor.GetValueFromBag(bag, cc);
                mask.radius = m_Radius.GetValueFromBag(bag, cc);
                mask.blur = m_Blur.GetValueFromBag(bag, cc);
                mask.useNormalizedMaskRect = m_UseNormalizedMaskRect.GetValueFromBag(bag, cc);
                mask.maskRect = new Rect(m_MaskRectX.GetValueFromBag(bag, cc), m_MaskRectY.GetValueFromBag(bag, cc), m_MaskRectWidth.GetValueFromBag(bag, cc), m_MaskRectHeight.GetValueFromBag(bag, cc));
            }
        }
    }
}
