using System;
using System.Collections.Generic;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A color swatch is a visual element that displays a color or a gradient.
    /// </summary>
    public class ColorSwatch : VisualElement, INotifyValueChanged<List<ColorEntry>>, ISizeableElement
    {
        const int k_MaxGradientSteps = 11;

        /// <summary>
        /// The ColorSwatch main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-colorswatch";

        /// <summary>
        /// The ColorSwatch image styling class.
        /// </summary>
        public static readonly string imageUssClassName = ussClassName + "__image";

        /// <summary>
        /// The ColorSwatch size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ColorSwatch round styling class.
        /// </summary>
        public static readonly string roundUssClassName = ussClassName + "--round";

        static readonly CustomStyleProperty<Color> k_UssCheckerColor1 = new CustomStyleProperty<Color>("--checker-color-1");

        static readonly CustomStyleProperty<Color> k_UssCheckerColor2 = new CustomStyleProperty<Color>("--checker-color-2");

        static readonly CustomStyleProperty<int> k_UssCheckerSize = new CustomStyleProperty<int>("--checker-size");

        List<ColorEntry> m_Value;

        static Material s_Material;

        Vector2 m_PreviousSize = Vector2.zero;

        readonly Image m_Image;

        RenderTexture m_RT;

        Color m_CheckerColor1;

        Color m_CheckerColor2;

        int m_CheckerSize;

        static readonly int k_CheckerColor1 = Shader.PropertyToID("_CheckerColor1");

        static readonly int k_CheckerColor2 = Shader.PropertyToID("_CheckerColor2");

        static readonly int k_CheckerSize = Shader.PropertyToID("_CheckerSize");

        static readonly int k_Width = Shader.PropertyToID("_Width");

        static readonly int k_Height = Shader.PropertyToID("_Height");

        static readonly int k_Count = Shader.PropertyToID("_Count");

        static readonly int k_Positions = Shader.PropertyToID("_Positions");

        static readonly int k_Colors = Shader.PropertyToID("_Colors");

        readonly List<Color> m_ColorList = new List<Color>();

        readonly List<float> m_PositionList = new List<float>();

        Size m_Size;

        /// <summary>
        /// The color entry list.
        /// </summary>
        public List<ColorEntry> value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;

                if (m_Value != null && value != null)
                {
                    if (m_Value.Count == value.Count)
                    {
                        var equal = true;
                        for (var i = 0; i < m_Value.Count; i++)
                        {
                            if (!m_Value[i].Equals(value[i]))
                            {
                                equal = false;
                                break;
                            }
                        }

                        if (equal)
                            return;
                    }
                }

                using var evt = ChangeEvent<List<ColorEntry>>.GetPooled(m_Value, value);
                SetValueWithoutNotify(value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The single color of the <see cref="ColorSwatch"/>.
        /// Setting this property will overwrite the current color entry list to contain only the given single color value.
        /// The property's getter always return the first item of the color entry list.
        /// </summary>
        public Color color
        {
            get => value is { Count: > 0 } ? value[0].color : default;
            set => this.value = new List<ColorEntry> { new ColorEntry(value, 0) };
        }

        /// <summary>
        /// The size of the <see cref="ColorSwatch"/> element.
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
        /// Round variant of the <see cref="ColorSwatch"/>.
        /// </summary>
        public bool round
        {
            get => ClassListContains(roundUssClassName);
            set => EnableInClassList(roundUssClassName, value);
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ColorSwatch()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;

            m_Image = new Image { name = imageUssClassName, pickingMode = PickingMode.Ignore };
            m_Image.AddToClassList(imageUssClassName);
            hierarchy.Add(m_Image);
            m_Image.StretchToParentSize();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);

            SetValueWithoutNotify(null);

            size = Size.M;
            round = false;
        }

        /// <summary>
        /// Add a color entry in the current swatch.
        /// This methods is useful to create gradients dynamically.
        /// </summary>
        /// <param name="newColor">The color to add.</param>
        /// <param name="position">At which position the given color should be added. The expected value must be normalized.</param>
        /// <returns>The index of the added color entry in the list.</returns>
        public int AddColor(Color newColor, float position)
        {
            var newVal = new List<ColorEntry>(value);
            var index = newVal.FindLastIndex(col => col.position < position) + 1;
            newVal.Insert(index, new ColorEntry(newColor, position));
            value = newVal;
            return index;
        }

        /// <summary>
        /// Remove a color entry from the current list.
        /// </summary>
        /// <param name="index">The index of the color entry to remove.</param>
        public void RemoveColor(int index)
        {
            var newVal = new List<ColorEntry>(value);
            if (index < newVal.Count && index >= 0)
            {
                newVal.RemoveAt(index);
                value = newVal;
            }
        }

        /// <summary>
        /// Force the refresh of the visual element.
        /// </summary>
        public void Refresh()
        {
            GenerateTextures();
        }

        /// <summary>
        /// Set the color entry list value, without being notified of any changes.
        /// </summary>
        /// <param name="newValue">The new color entry list.</param>
        public void SetValueWithoutNotify(List<ColorEntry> newValue)
        {
            m_Value = newValue;
            GenerateTextures();
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            GenerateTextures();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (m_RT)
                RenderTexture.ReleaseTemporary(m_RT);

            m_RT = null;
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
            }

            SetValueWithoutNotify(value);
        }

        void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(k_UssCheckerColor1, out var checkerColor1)
                && evt.customStyle.TryGetValue(k_UssCheckerColor2, out var checkerColor2)
                && evt.customStyle.TryGetValue(k_UssCheckerSize, out var checkerSize))
            {
                m_CheckerColor1 = checkerColor1;
                m_CheckerColor2 = checkerColor2;
                m_CheckerSize = checkerSize;
                GenerateTextures();
            }
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
                s_Material = new Material(Shader.Find("Hidden/App UI/ColorSwatch")) { hideFlags = HideFlags.HideAndDontSave };
            }

            m_ColorList.Clear();
            m_PositionList.Clear();
            if (m_Value != null)
            {
                foreach (var v in m_Value)
                {
                    m_ColorList.Add(v.color);
                    m_PositionList.Add(v.position);
                }
            }
            while (m_ColorList.Count < k_MaxGradientSteps)
            {
                m_ColorList.Add(new Color());
                m_PositionList.Add(-1);
            }

            s_Material.SetColorArray(k_Colors, m_ColorList.ToArray());
            s_Material.SetFloatArray(k_Positions, m_PositionList.ToArray());
            s_Material.SetInt(k_Count, m_Value?.Count ?? 0);
            s_Material.SetColor(k_CheckerColor1, m_CheckerColor1);
            s_Material.SetColor(k_CheckerColor2, m_CheckerColor2);
            s_Material.SetFloat(k_CheckerSize, m_CheckerSize);
            s_Material.SetFloat(k_Width, rect.width);
            s_Material.SetFloat(k_Height, rect.height);

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;

            if (m_Image.image != m_RT)
                m_Image.image = m_RT;
            m_Image.MarkDirtyRepaint();
        }

        /// <summary>
        /// Instantiates an <see cref="ColorSwatch"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ColorSwatch, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ColorSwatch"/>.
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
                defaultValue = Color.clear,
            };

            readonly UxmlBoolAttributeDescription m_Round = new UxmlBoolAttributeDescription
            {
                name = "round",
                defaultValue = false,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (ColorSwatch)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.round = m_Round.GetValueFromBag(bag, cc);
                var c = Color.clear;
                if (m_Color.TryGetValueFromBag(bag, cc, ref c))
                {
                    element.color = c;
                }
            }
        }
    }
}
