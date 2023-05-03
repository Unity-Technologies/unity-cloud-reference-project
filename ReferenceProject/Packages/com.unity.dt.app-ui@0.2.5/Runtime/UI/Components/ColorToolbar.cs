using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A toolbar that contains a color swatch for the previous color, a color swatch for the current color, and an eye dropper button.
    /// </summary>
    public class ColorToolbar : VisualElement
    {
        /// <summary>
        /// The main Uss class name of this element.
        /// </summary>
        public const string ussClassName = "appui-colortoolbar";

        /// <summary>
        /// The Uss class name of the eye dropper button.
        /// </summary>
        public const string eyeDropperUssClassName = ussClassName + "__eyedropper";

        /// <summary>
        /// The Uss class name of the swatch container.
        /// </summary>
        public const string swatchContainerUssClassName = ussClassName + "__swatchcontainer";

        /// <summary>
        /// The Uss class name of the previous color swatch.
        /// </summary>
        public const string previousColorSwatchUssClassName = ussClassName + "__previouscolorswatch";

        /// <summary>
        /// The Uss class name of the current color swatch.
        /// </summary>
        public const string currentColorSwatchUssClassName = ussClassName + "__currentcolorswatch";

        /// <summary>
        /// The event that is invoked when the previous color swatch is clicked.
        /// </summary>
        public event Action previousColorSwatchClicked;

        readonly ActionButton m_EyeDropperButton;

        readonly VisualElement m_SwatchContainer;

        readonly ColorSwatch m_PreviousColorSwatch;

        readonly ColorSwatch m_CurrentColorSwatch;

        readonly Clickable m_PreviousColorSwatchClickable;

        /// <summary>
        /// The previous color swatch value.
        /// </summary>
        public Color previousColor
        {
            get => m_PreviousColorSwatch.color;
            set => m_PreviousColorSwatch.color = value;
        }

        /// <summary>
        /// The current color swatch value.
        /// </summary>
        public Color currentColor
        {
            get => m_CurrentColorSwatch.color;
            set => m_CurrentColorSwatch.color = value;
        }

        /// <summary>
        /// The eye dropper button.
        /// </summary>
        public ActionButton eyeDropperButton => m_EyeDropperButton;

        /// <summary>
        /// The previous color swatch.
        /// </summary>
        public ColorSwatch previousColorSwatch => m_PreviousColorSwatch;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorToolbar()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            m_EyeDropperButton = new ActionButton { name = eyeDropperUssClassName, focusable = true, pickingMode = PickingMode.Position, icon = "color-picker" };
            m_EyeDropperButton.AddToClassList(eyeDropperUssClassName);
            hierarchy.Add(m_EyeDropperButton);

            m_SwatchContainer = new VisualElement { name = swatchContainerUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
            m_SwatchContainer.AddToClassList(swatchContainerUssClassName);
            hierarchy.Add(m_SwatchContainer);

            m_PreviousColorSwatch = new ColorSwatch { name = previousColorSwatchUssClassName, focusable = true, pickingMode = PickingMode.Position };
            m_PreviousColorSwatch.AddToClassList(previousColorSwatchUssClassName);
            m_SwatchContainer.hierarchy.Add(m_PreviousColorSwatch);

            m_CurrentColorSwatch = new ColorSwatch { name = currentColorSwatchUssClassName, focusable = false, pickingMode = PickingMode.Ignore };
            m_CurrentColorSwatch.AddToClassList(currentColorSwatchUssClassName);
            m_SwatchContainer.hierarchy.Add(m_CurrentColorSwatch);

            m_PreviousColorSwatchClickable = new Submittable(OnPreviousSwatchClicked);
            m_PreviousColorSwatch.AddManipulator(m_PreviousColorSwatchClickable);
        }

        void OnPreviousSwatchClicked()
        {
            previousColorSwatchClicked?.Invoke();
        }
    }
}
