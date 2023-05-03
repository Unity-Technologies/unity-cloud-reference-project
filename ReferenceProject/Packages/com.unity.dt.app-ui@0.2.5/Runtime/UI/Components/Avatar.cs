using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Avatar UI element.
    /// </summary>
    public class Avatar : VisualElement, ISizeableElement
    {
        /// <summary>
        /// The Avatar main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-avatar";

        /// <summary>
        /// The Avatar container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Avatar notification badge styling class.
        /// </summary>
        public static readonly string notificationBadgeUssClassName = ussClassName + "__notification";

        /// <summary>
        /// The Avatar notification container styling class.
        /// </summary>
        public static readonly string notificationContainerBadgeUssClassName = ussClassName + "__notificationcontainer";

        /// <summary>
        /// The Avatar icon container styling class.
        /// </summary>
        public static readonly string iconContainerUssClassName = ussClassName + "__iconcontainer";

        /// <summary>
        /// The Avatar icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The Avatar label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The Avatar size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        Size m_Size = Size.M;

        readonly VisualElement m_IconContainer;

        readonly VisualElement m_NotificationContainer;

        readonly ExVisualElement m_Container;

        readonly LocalizedTextElement m_LabelElement;

        /// <summary>
        /// The content container of the Avatar.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The Avatar size.
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
        /// The Avatar notification badge.
        /// </summary>
        public Badge notificationBadge { get; private set; }

        /// <summary>
        /// Set the Avatar notification badge visibility.
        /// </summary>
        public bool withNotification
        {
            get => !notificationBadge.ClassListContains(Styles.hiddenUssClassName);
            set => notificationBadge.EnableInClassList(Styles.hiddenUssClassName, !value);
        }

        /// <summary>
        /// The Avatar icon.
        /// </summary>
        public Icon icon { get; private set; }

        /// <summary>
        /// The Avatar outline color.
        /// </summary>
        public Color outlineColor
        {
            get => m_Container.outlineColor ?? Color.clear;
            set => m_Container.outlineColor = value;
        }

        /// <summary>
        /// The Avatar text color.
        /// </summary>
        public Color textColor
        {
            get => m_LabelElement.resolvedStyle.color;
            set => m_LabelElement.style.color = value;
        }

        /// <summary>
        /// The Avatar background color.
        /// </summary>
        public Color backgroundColor
        {
            get => m_Container.resolvedStyle.backgroundColor;
            set => m_Container.style.backgroundColor = value;
        }

        /// <summary>
        /// The Avatar text.
        /// </summary>
        public string text
        {
            get => m_LabelElement.text;
            set
            {
                m_LabelElement.text = value;
                m_LabelElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// The Avatar background image.
        /// </summary>
        public StyleBackground backgroundImage
        {
            get => m_Container.resolvedStyle.backgroundImage;
            set => m_Container.style.backgroundImage = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Avatar()
        {
            AddToClassList(ussClassName); ;
            pickingMode = PickingMode.Position;
            focusable = false;

            m_Container = new ExVisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            m_LabelElement = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);
            m_Container.hierarchy.Add(m_LabelElement);

            m_IconContainer = new VisualElement { name = iconContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_IconContainer.AddToClassList(iconContainerUssClassName);
            m_Container.hierarchy.Add(m_IconContainer);

            icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore, primary = true };
            icon.AddToClassList(iconUssClassName);
            m_IconContainer.hierarchy.Add(icon);

            m_NotificationContainer = new VisualElement { name = notificationContainerBadgeUssClassName, pickingMode = PickingMode.Ignore };
            m_NotificationContainer.AddToClassList(notificationContainerBadgeUssClassName);
            m_Container.hierarchy.Add(m_NotificationContainer);

            notificationBadge = new Badge { name = notificationBadgeUssClassName, pickingMode = PickingMode.Ignore };
            notificationBadge.AddToClassList(notificationBadgeUssClassName);
            m_NotificationContainer.hierarchy.Add(notificationBadge);

            size = Size.M;
            backgroundColor = Color.gray;
            textColor = Color.white;
            text = null;
            icon.iconName = null;
            withNotification = false;
            notificationBadge.text = null;
            outlineColor = Color.clear;
        }

        /// <summary>
        /// Defines the UxmlFactory for the Avatar.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Avatar, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Avatar"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlColorAttributeDescription m_BackgroundColor = new UxmlColorAttributeDescription
            {
                name = "background-color",
                defaultValue = Color.gray
            };

            readonly UxmlColorAttributeDescription m_TextColor = new UxmlColorAttributeDescription
            {
                name = "text-color",
                defaultValue = Color.white
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };

            readonly UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
            {
                name = "text",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Icon = new UxmlStringAttributeDescription
            {
                name = "icon",
                defaultValue = null
            };

            readonly UxmlBoolAttributeDescription m_WithNotification = new UxmlBoolAttributeDescription
            {
                name = "with-notification",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_NotificationText = new UxmlStringAttributeDescription
            {
                name = "notification-text",
                defaultValue = null
            };

            readonly UxmlColorAttributeDescription m_OutlineColor = new UxmlColorAttributeDescription
            {
                name = "outline-color",
                defaultValue = Color.clear
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

                var element = (Avatar)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.text = m_Text.GetValueFromBag(bag, cc);
                element.textColor = m_TextColor.GetValueFromBag(bag, cc);
                element.backgroundColor = m_BackgroundColor.GetValueFromBag(bag, cc);
                element.outlineColor = m_OutlineColor.GetValueFromBag(bag, cc);
                element.icon.iconName = m_Icon.GetValueFromBag(bag, cc);
                element.withNotification = m_WithNotification.GetValueFromBag(bag, cc);
                element.notificationBadge.text = m_NotificationText.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
