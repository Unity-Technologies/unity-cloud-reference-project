using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// SVSquare UI element. It is a square that allows to select a color by selecting a point in a 2D space.
    /// <para />
    /// The X axis represents the Hue and the Y axis represents the Saturation.
    /// </summary>
    public class SVSquare : VisualElement, INotifyValueChanging<Vector2>
    {
        /// <summary>
        /// The SVSquare main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-svsquare";

        /// <summary>
        /// The SVSquare image styling class.
        /// </summary>
        public static readonly string imageUssClassName = ussClassName + "__image";

        /// <summary>
        /// The SVSquare thumb styling class.
        /// </summary>
        public static readonly string thumbUssClassName = ussClassName + "__thumb";

        /// <summary>
        /// The SVSquare thumb swatch styling class.
        /// </summary>
        public static readonly string thumbSwatchUssClassName = ussClassName + "__thumbswatch";

        static readonly int k_Color = Shader.PropertyToID("_Color");

        Vector2 m_Value;

        readonly ExVisualElement m_Thumb;

        readonly Draggable m_DragManipulator;

        readonly Image m_Image;

        float m_RefHue;

        RenderTexture m_RT;

        static Material s_Material;

        Vector2 m_PreviousSize;

        Vector2 m_PreviousValue;

        /// <summary>
        /// Selected brightness value.
        /// </summary>
        public float brightness
        {
            get => m_Value.y;
            set => this.value = new Vector2(this.value.x, Mathf.Clamp01(value));
        }

        /// <summary>
        /// The reference color (current hue with the maximum brightness and saturation).
        /// </summary>
        public Color referenceColor => Color.HSVToRGB(referenceHue, 1, 1);

        /// <summary>
        /// The current hue used to display the SV Square.
        /// </summary>
        public float referenceHue
        {
            get => m_RefHue;
            set
            {
                m_RefHue = value;
                GenerateTextures();
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// Selected saturation value.
        /// </summary>
        public float saturation
        {
            get => m_Value.x;
            set => this.value = new Vector2(Mathf.Clamp01(value), this.value.y);
        }

        /// <summary>
        /// The currently selected color.
        /// </summary>
        public Color selectedColor => Color.HSVToRGB(referenceHue, saturation, brightness);

        /// <summary>
        /// The current value of the SV Square. The x component is the saturation and the y component is the brightness.
        /// </summary>
        public Vector2 value
        {
            get => m_Value;
            set
            {
                var validValue = new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));

                if (m_Value == validValue)
                    return;

                using var evt = ChangeEvent<Vector2>.GetPooled(m_Value, validValue);
                evt.target = this;
                SetValueWithoutNotify(validValue);
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The increment factor used when the user uses the keyboard to change the value.
        /// </summary>
        public float incrementFactor { get; set; } = 0.01f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SVSquare()
        {
            pickingMode = PickingMode.Position;
            focusable = true;

            AddToClassList(ussClassName);

            m_Image = new Image { name = imageUssClassName, pickingMode = PickingMode.Ignore, focusable = false };
            m_Image.AddToClassList(imageUssClassName);
            hierarchy.Add(m_Image);

            m_Thumb = new ExVisualElement
            {
                name = thumbUssClassName,
                pickingMode = PickingMode.Position,
                usageHints = UsageHints.DynamicTransform | UsageHints.DynamicColor,
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders |
                    ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows,
            };
            m_Thumb.AddToClassList(thumbUssClassName);
            hierarchy.Add(m_Thumb);

            m_DragManipulator = new Draggable(OnClicked, OnPointerMove, OnPointerUp, OnPointerDown);
            this.AddManipulator(m_DragManipulator);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            SetValueWithoutNotify(Vector2.zero);
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Thumb.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders |
                ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Thumb.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders |
                ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows | ExVisualElement.Passes.Outline;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;
            var previousValue = value;
            var val = previousValue;

            if (evt.keyCode == KeyCode.LeftArrow)
            {
                val.x -= incrementFactor;
                handled = true;
            }

            if (evt.keyCode == KeyCode.RightArrow)
            {
                val.x += incrementFactor;
                handled = true;
            }

            if (evt.keyCode == KeyCode.DownArrow)
            {
                val.y -= incrementFactor;
                handled = true;
            }

            if (evt.keyCode == KeyCode.UpArrow)
            {
                val.y += incrementFactor;
                handled = true;
            }

            if (handled)
            {
                evt.PreventDefault();
                evt.StopPropagation();

                var validValue = new Vector2(Mathf.Clamp01(val.x), Mathf.Clamp01(val.y));

                if (value == validValue)
                    return;

                SetValueWithoutNotify(validValue);

                using var changingEvt = ChangingEvent<Vector2>.GetPooled();
                changingEvt.previousValue = previousValue;
                changingEvt.newValue = validValue;
                changingEvt.target = this;
                SendEvent(changingEvt);
            }
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

        void OnClicked()
        {
            if (!m_DragManipulator.hasMoved)
            {
                OnPointerMove(m_DragManipulator);
                OnPointerUp(m_DragManipulator);
            }
        }

        void OnPointerMove(Draggable draggable)
        {
            var previousValue = value;
            var val = ComputeValue(draggable.localPosition);

            if (previousValue != val)
            {
                SetValueWithoutNotify(val);

                using var evt = ChangingEvent<Vector2>.GetPooled();
                evt.previousValue = previousValue;
                evt.newValue = val;
                evt.target = this;
                SendEvent(evt);
            }
        }

        void OnPointerUp(Draggable draggable)
        {
            if (m_PreviousValue == value)
                return;

            using var evt = ChangeEvent<Vector2>.GetPooled(m_PreviousValue, value);
            evt.target = this;
            SendEvent(evt);
        }

        void OnPointerDown(Draggable obj)
        {
            m_PreviousValue = value;
        }

        /// <summary>
        /// Sets the value without notifying the user of the change.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(Vector2 newValue)
        {
            m_Value = newValue;
            m_Thumb.style.top = (paddingRect.height - brightness * paddingRect.height) - m_Thumb.resolvedStyle.height * 0.5f;
            m_Thumb.style.left = saturation * paddingRect.width - m_Thumb.resolvedStyle.width * 0.5f;
            m_Thumb.backgroundColor = selectedColor;
        }

        Vector2 ComputeValue(Vector2 localPosition)
        {
            var sat = Mathf.Clamp01(localPosition.x / paddingRect.width);
            var bri = 1f - Mathf.Clamp01(localPosition.y / paddingRect.height);
            return new Vector2(sat, bri);
        }

        void GenerateTextures()
        {
            var rect = contentRect;

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
                s_Material = new Material(Shader.Find("Hidden/App UI/SVSquare")) { hideFlags = HideFlags.HideAndDontSave };
            }

            s_Material.SetColor(k_Color, QualitySettings.activeColorSpace == ColorSpace.Gamma ? referenceColor : referenceColor.gamma);

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;

            if (m_Image.image != m_RT)
                m_Image.image = m_RT;
            m_Image.MarkDirtyRepaint();
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="SVSquare"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SVSquare, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SVSquare"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlFloatAttributeDescription m_Brightness = new UxmlFloatAttributeDescription
            {
                name = "brightness",
                defaultValue = 1,
                restriction = new UxmlValueBounds { min = "0", max = "1" }
            };

            readonly UxmlFloatAttributeDescription m_ReferenceHue = new UxmlFloatAttributeDescription
            {
                name = "reference-hue",
                defaultValue = 0,
                restriction = new UxmlValueBounds { min = "0", max = "1" }
            };

            readonly UxmlFloatAttributeDescription m_Saturation = new UxmlFloatAttributeDescription
            {
                name = "saturation",
                defaultValue = 1,
                restriction = new UxmlValueBounds { min = "0", max = "1" }
            };

            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
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

                var square = (SVSquare)ve;
                square.referenceHue = m_ReferenceHue.GetValueFromBag(bag, cc);
                var satBri = Vector2.one;
                satBri.x = m_Saturation.GetValueFromBag(bag, cc);
                satBri.y = m_Brightness.GetValueFromBag(bag, cc);
                square.SetValueWithoutNotify(satBri);
                square.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
