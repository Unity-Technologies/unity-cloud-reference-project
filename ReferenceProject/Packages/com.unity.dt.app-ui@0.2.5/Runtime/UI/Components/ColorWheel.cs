using System;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A color wheel that allows the user to select a color hue by rotating the wheel.
    /// It is also possible to set the saturation and brightness and opacity of the wheel.
    /// </summary>
    public class ColorWheel : VisualElement, INotifyValueChanging<float>
    {
        const float k_InvTwoPI = 0.15915494309f;

        const float k_TwoPI = 6.28318530718f;

        /// <summary>
        /// The ColorWheel main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-colorwheel";

        /// <summary>
        /// The ColorWheel image styling class.
        /// </summary>
        public static readonly string imageUssClassName = ussClassName + "__image";

        /// <summary>
        /// The ColorWheel thumb styling class.
        /// </summary>
        public static readonly string thumbUssClassName = ussClassName + "__thumb";

        /// <summary>
        /// The ColorWheel thumb swatch styling class.
        /// </summary>
        public static readonly string thumbSwatchUssClassName = ussClassName + "__thumbswatch";

        static readonly CustomStyleProperty<Color> k_UssCheckerColor1 = new CustomStyleProperty<Color>("--checker-color-1");

        static readonly CustomStyleProperty<Color> k_UssCheckerColor2 = new CustomStyleProperty<Color>("--checker-color-2");

        static readonly CustomStyleProperty<int> k_UssCheckerSize = new CustomStyleProperty<int>("--checker-size");

        static readonly CustomStyleProperty<float> k_UssOpacity = new CustomStyleProperty<float>("--opacity");

        static readonly CustomStyleProperty<float> k_UssBrightness = new CustomStyleProperty<float>("--brightness");

        static readonly CustomStyleProperty<float> k_UssSaturation = new CustomStyleProperty<float>("--saturation");

        static readonly CustomStyleProperty<float> k_UssInnerRadius = new CustomStyleProperty<float>("--inner-radius");

        static readonly int k_CheckerColor1 = Shader.PropertyToID("_CheckerColor1");

        static readonly int k_CheckerColor2 = Shader.PropertyToID("_CheckerColor2");

        static readonly int k_CheckerSize = Shader.PropertyToID("_CheckerSize");

        static readonly int k_Width = Shader.PropertyToID("_Width");

        static readonly int k_Height = Shader.PropertyToID("_Height");

        static readonly int k_InnerRadius = Shader.PropertyToID("_InnerRadius");

        static readonly int k_Saturation = Shader.PropertyToID("_Saturation");

        static readonly int k_Brightness = Shader.PropertyToID("_Brightness");

        static readonly int k_Opacity = Shader.PropertyToID("_Opacity");

        static readonly int k_AA = Shader.PropertyToID("_AA");

        readonly Image m_Image;

        Color m_CheckerColor1;

        Color m_CheckerColor2;

        int m_CheckerSize;

        RenderTexture m_RT;

        Vector2 m_PreviousSize;

        float m_Value;

        static Material s_Material;

        float m_Opacity = 1f;

        float m_Brightness = 1f;

        float m_Saturation = 1f;

        float m_InnerRadius = 0.4f;

        readonly Draggable m_DraggerManipulator;

        readonly ExVisualElement m_Thumb;

        float m_PreviousValue;

        /// <summary>
        /// The hue value of the color wheel.
        /// </summary>
        public float value
        {
            get => m_Value;
            set
            {
                var validValue = Mathf.Repeat(value, 1f);
                if (Mathf.Approximately(m_Value, validValue))
                    return;
                using var evt = ChangeEvent<float>.GetPooled(m_Value, validValue);
                SetValueWithoutNotify(validValue);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The opacity of the color wheel. Note that a checkerboard pattern is always drawn behind the color wheel.
        /// </summary>
        public float opacity
        {
            get => m_Opacity;
            set
            {
                var validatedValue = Mathf.Clamp01(value);
                if (Mathf.Approximately(validatedValue, m_Opacity))
                    return;
                m_Opacity = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The brightness of the color wheel.
        /// </summary>
        public float brightness
        {
            get => m_Brightness;
            set
            {
                var validatedValue = Mathf.Clamp01(value);
                if (Mathf.Approximately(validatedValue, m_Brightness))
                    return;
                m_Brightness = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The saturation of the color wheel.
        /// </summary>
        public float saturation
        {
            get => m_Saturation;
            set
            {
                var validatedValue = Mathf.Clamp01(value);
                if (Mathf.Approximately(validatedValue, m_Saturation))
                    return;
                m_Saturation = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The inner radius of the color wheel.
        /// </summary>
        public float innerRadius
        {
            get => m_InnerRadius;
            set
            {
                var validatedValue = Mathf.Clamp(value, 0, 0.499f);
                if (Mathf.Approximately(validatedValue, m_InnerRadius))
                    return;
                m_InnerRadius = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The size of the checkerboard pattern.
        /// </summary>
        public int checkerSize
        {
            get => m_CheckerSize;
            set
            {
                m_CheckerSize = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The first color of the checkerboard pattern.
        /// </summary>
        public Color checkerColor1
        {
            get => m_CheckerColor1;
            set
            {
                m_CheckerColor1 = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The second color of the checkerboard pattern.
        /// </summary>
        public Color checkerColor2
        {
            get => m_CheckerColor2;
            set
            {
                m_CheckerColor2 = value;
                GenerateTextures();
            }
        }

        /// <summary>
        /// The currently selected color.
        /// </summary>
        public Color selectedColor => Color.HSVToRGB(value, saturation, brightness);

        /// <summary>
        /// The factor by which the value is incremented when interacting with the wheel from the keyboard.
        /// </summary>
        public float incrementFactor { get; set; } = 0.01f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorWheel()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;

            m_Image = new Image { name = imageUssClassName, pickingMode = PickingMode.Ignore };
            m_Image.AddToClassList(imageUssClassName);
            hierarchy.Add(m_Image);

            m_Thumb = new ExVisualElement
            {
                name = thumbUssClassName,
                pickingMode = PickingMode.Position,
                usageHints = UsageHints.DynamicTransform,
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders | ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows
            };
            m_Thumb.AddToClassList(thumbUssClassName);
            hierarchy.Add(m_Thumb);

            m_DraggerManipulator = new Draggable(OnTrackClicked, OnTrackDragged, OnTrackUp, OnTrackDown);
            this.AddManipulator(m_DraggerManipulator);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            SetValueWithoutNotify(0);
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Thumb.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders | ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Thumb.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders | ExVisualElement.Passes.BackgroundColor | ExVisualElement.Passes.OutsetShadows | ExVisualElement.Passes.Outline;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;
            var previousValue = value;
            var val = previousValue;

            if (evt.keyCode is KeyCode.LeftArrow or KeyCode.DownArrow)
            {
                val -= incrementFactor;
                handled = true;
            }

            if (evt.keyCode is KeyCode.RightArrow or KeyCode.UpArrow)
            {
                val += incrementFactor;
                handled = true;
            }

            if (handled)
            {
                evt.PreventDefault();
                evt.StopPropagation();

                val = Mathf.Repeat(val, 1f);
                SetValueWithoutNotify(val);

                using var changingEvt = ChangingEvent<float>.GetPooled();
                changingEvt.previousValue = previousValue;
                changingEvt.newValue = val;
                changingEvt.target = this;
                SendEvent(changingEvt);
            }
        }

        void OnTrackClicked()
        {
            if (!m_DraggerManipulator.hasMoved)
            {
                OnTrackDragged(m_DraggerManipulator);
                OnTrackUp(m_DraggerManipulator);
            }
        }

        void OnTrackDown(Draggable obj)
        {
            m_PreviousValue = value;
        }

        void OnTrackUp(Draggable obj)
        {
            if (Mathf.Approximately(value, m_PreviousValue))
                return;

            using var evt = ChangeEvent<float>.GetPooled(m_PreviousValue, value);
            evt.target = this;
            SendEvent(evt);

        }

        void OnTrackDragged(Draggable obj)
        {
            var val = ComputeValue(m_DraggerManipulator.localPosition);
            val = Mathf.Repeat(val, 1f);
            SetValueWithoutNotify(val);

            using var evt = ChangingEvent<float>.GetPooled();
            evt.previousValue = m_PreviousValue;
            evt.newValue = val;
            evt.target = this;
            SendEvent(evt);
        }

        void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            var changed = false;

            if (evt.customStyle.TryGetValue(k_UssCheckerColor1, out var ussCheckerColor1))
            {
                m_CheckerColor1 = ussCheckerColor1;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssCheckerColor2, out var ussCheckerColor2))
            {
                m_CheckerColor2 = ussCheckerColor2;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssCheckerSize, out var ussCheckerSize))
            {
                m_CheckerSize = ussCheckerSize;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssOpacity, out var ussOpacity))
            {
                m_Opacity = ussOpacity;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssBrightness, out var ussBrightness))
            {
                m_Brightness = ussBrightness;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssSaturation, out var ussSaturation))
            {
                m_Saturation = ussSaturation;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssInnerRadius, out var ussInnerRadius))
            {
                m_InnerRadius = ussInnerRadius;
                changed = true;
            }

            if (changed)
            {
                GenerateTextures();
                SetValueWithoutNotify(value);
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

        /// <summary>
        /// Sets the value without sending any event.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(float newValue)
        {
            m_Value = newValue;

            var rect = m_Image.contentRect;

            if (!rect.IsValid())
                return;

            var center = rect.size * 0.5f;
            var refPoint = new Vector2(1, 0);
            var targetAngle = Mathf.Atan2(refPoint.y, refPoint.x) - m_Value * k_TwoPI;
            const float maxRadius = 0.5f;
            var selectorSize = m_Thumb.resolvedStyle.width;
            var radius = Mathf.Min(
                rect.width * ((maxRadius - innerRadius) * 0.5f + innerRadius) - 0.65f,
                rect.height * ((maxRadius - innerRadius) * 0.5f + innerRadius) - 0.65f);
            var x = center.x + radius * Mathf.Cos(targetAngle);
            var y = center.y + radius * Mathf.Sin(targetAngle);
            m_Thumb.style.top = y - selectorSize * 0.5f;
            m_Thumb.style.left = x - selectorSize * 0.5f;
        }

        void GenerateTextures()
        {
            var rect = m_Image.contentRect;

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
                s_Material = new Material(Shader.Find("Hidden/App UI/ColorWheel")) { hideFlags = HideFlags.HideAndDontSave };
            }

            s_Material.SetColor(k_CheckerColor1, m_CheckerColor1);
            s_Material.SetColor(k_CheckerColor2, m_CheckerColor2);
            s_Material.SetFloat(k_CheckerSize, m_CheckerSize);
            s_Material.SetFloat(k_Width, rect.width);
            s_Material.SetFloat(k_Height, rect.height);
            s_Material.SetFloat(k_AA, 2.0f / texSize.x);

            s_Material.SetFloat(k_InnerRadius, innerRadius);
            s_Material.SetFloat(k_Saturation, saturation);
            s_Material.SetFloat(k_Brightness, brightness);
            s_Material.SetFloat(k_Opacity, opacity);

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;

            if (m_Image.image != m_RT)
                m_Image.image = m_RT;
            m_Image.MarkDirtyRepaint();
        }

        float ComputeValue(Vector2 localPosition)
        {
            var center = new Vector2(paddingRect.width * 0.5f, paddingRect.height * 0.5f);
            var direction = (localPosition - center).normalized;

            // simplified since atan of red color is 0.
            // var refPoint = new Vector2(1, 0); // angle = 0 = red = position(1,0) in wheel
            return (/*Mathf.Atan2(refPoint.y, refPoint.x)*/ -Mathf.Atan2(direction.y, direction.x)) * k_InvTwoPI;
        }

        /// <summary>
        /// Instantiates an <see cref="ColorWheel"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ColorWheel, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ColorWheel"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlFloatAttributeDescription m_Value = new UxmlFloatAttributeDescription
            {
                name = "value",
                defaultValue = 0
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

                var el = (ColorWheel)ve;
                el.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
