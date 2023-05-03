using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// IconButton UI element.
    /// </summary>
    public class IconButton : ExVisualElement, ISizeableElement
    {
        /// <summary>
        /// The IconButton main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-button";

        /// <summary>
        /// The IconButton primary variant styling class.
        /// </summary>
        public static readonly string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The IconButton quiet mode styling class.
        /// </summary>
        public static readonly string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The IconButton leading container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__leadingcontainer";

        /// <summary>
        /// The IconButton leading icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__leadingicon";

        /// <summary>
        /// The IconButton size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly VisualElement m_Container;

        readonly Icon m_Icon;

        Size m_Size;

        Clickable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IconButton()
            : this(null)
        {

        }

        /// <summary>
        /// Construct an IconButton with a given icon.
        /// </summary>
        /// <param name="iconName">The name of the icon.</param>
        /// <param name="clickEvent">The click event callback.</param>
        public IconButton(string iconName, Action clickEvent = null)
        {
            AddToClassList(ussClassName);
            AddToClassList(Button.iconOnlyUssClassName);

            clickable = new Submittable(clickEvent);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            passMask = 0;

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);

            m_Container.hierarchy.Add(m_Icon);
            hierarchy.Add(m_Container);

            primary = false;
            quiet = false;
            icon = iconName;
            size = Size.M;

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// Event triggered when the Button has been clicked.
        /// </summary>
        public event Action clicked
        {
            add => m_Clickable.clicked += value;
            remove => m_Clickable.clicked -= value;
        }

        /// <summary>
        /// Clickable Manipulator for this Button.
        /// </summary>
        public Clickable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// Use the primary variant of the Button.
        /// </summary>
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set => EnableInClassList(primaryUssClassName, value);
        }

        /// <summary>
        /// The quiet state of the Button.
        /// </summary>
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set => EnableInClassList(quietUssClassName, value);
        }

        /// <summary>
        /// The IconButton icon.
        /// </summary>
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                m_Icon.iconName = value;
                m_Container.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Icon.iconName));
            }
        }

        /// <summary>
        /// The Button size.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                m_Icon.size = m_Size switch
                {
                    Size.S => IconSize.S,
                    Size.M => IconSize.M,
                    Size.L => IconSize.L,
                    _ => IconSize.M
                };
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
            }
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="IconButton"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="IconButton"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_Icon = new UxmlStringAttributeDescription
            {
                name = "icon",
                defaultValue = null
            };

            readonly UxmlBoolAttributeDescription m_Primary = new UxmlBoolAttributeDescription
            {
                name = "primary",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Quiet = new UxmlBoolAttributeDescription
            {
                name = "quiet",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
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

                var element = (IconButton)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.primary = m_Primary.GetValueFromBag(bag, cc);
                element.quiet = m_Quiet.GetValueFromBag(bag, cc);
                element.icon = m_Icon.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
