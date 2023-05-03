using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// This is the main UI element of any Runtime App. The <see cref="Panel"/> class will create different
    /// UI layers for the main user-interface, popups, notifications and tooltips.
    /// It also inherits from <see cref="ContextProvider"/>, hence this element must not have
    /// any <see cref="ContextProvider"/> ancestors.
    /// </summary>
    public class Panel : ContextProvider
    {
        /// <summary>
        /// The name of the main UI layer.
        /// </summary>
        public const string mainContainerName = "main-container";

        /// <summary>
        /// The name of the Popups layer.
        /// </summary>
        public const string popupContainerName = "popup-container";

        /// <summary>
        /// The name of the Notifications layer.
        /// </summary>
        public const string notificationContainerName = "notification-container";

        /// <summary>
        /// The name of the Tooltip layer.
        /// </summary>
        public const string tooltipContainerName = "tooltip-container";

        readonly VisualElement m_MainContainer;

        readonly VisualElement m_NotificationContainer;

        readonly VisualElement m_PopupContainer;

        readonly VisualElement m_TooltipContainer;

        readonly List<Popup> m_DismissablePopups = new List<Popup>();

        TooltipManipulator m_TooltipManipulator;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Panel()
        {
            // Add a layer for the main UI
            m_MainContainer = new VisualElement { name = mainContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_MainContainer);
            hierarchy.Add(m_MainContainer);

            // Add a layer for popups stack (popovers, modals, trays)
            m_PopupContainer = new VisualElement { name = popupContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_PopupContainer);
            hierarchy.Add(m_PopupContainer);

            // Add a layer for notifications (snackbars, toasts)
            m_NotificationContainer = new VisualElement { name = notificationContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_NotificationContainer);
            m_NotificationContainer.style.flexDirection = FlexDirection.Column;
            m_NotificationContainer.style.alignItems = Align.Center;
            m_NotificationContainer.style.justifyContent = Justify.Center;
            hierarchy.Add(m_NotificationContainer);

            // Add a layer for tooltips
            m_TooltipContainer = new VisualElement { name = tooltipContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_TooltipContainer);
            hierarchy.Add(m_TooltipContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);

            RegisterCallback<DpiChangedEvent>(OnDpiChanged);

            scale = Application.isMobilePlatform ? "large" : "medium";
            theme = "dark";
            preferredTooltipPlacement = Tooltip.defaultPlacement;
        }

        void OnDpiChanged(DpiChangedEvent evt) 
        {
            Debug.Log($"DPI Changed from {evt.previousValue} to {evt.newValue}");
        }

        /// <summary>
        /// The application context.
        /// <remarks>
        /// This property is overriden for this class since the <see cref="Panel"/> must be the root context.
        /// </remarks>
        /// </summary>
        public override ApplicationContext context
        {
            get
            {
                // Check if there's no other ContextProvider as ancestor.
                var parentContextProvider = GetFirstAncestorOfType<ContextProvider>();
                Assert.IsNull(parentContextProvider, "An Application element should not be nested into others ContextProvider inside the visual tree.");

                // Return the main context of the application directly
                return new ApplicationContext(this);
            }
        }

        /// <summary>
        /// The main UI layer container.
        /// </summary>
        public override VisualElement contentContainer => m_MainContainer;

        /// <summary>
        /// The Popups layer container.
        /// </summary>
        public VisualElement popupContainer => m_PopupContainer;

        /// <summary>
        /// The Notifications layer container.
        /// </summary>
        public VisualElement notificationContainer => m_NotificationContainer;

        /// <summary>
        /// The Tooltip layer container.
        /// </summary>
        public VisualElement tooltipContainer => m_TooltipContainer;

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel != null)
            {
                if (m_TooltipManipulator != null)
                    this.RemoveManipulator(m_TooltipManipulator);
                AppUI.UnregisterPanel(this);
            }
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                if (m_TooltipManipulator == null)
                {
                    m_TooltipManipulator = new TooltipManipulator();
                    this.AddManipulator(m_TooltipManipulator);
                }

                AppUI.RegisterPanel(this);
            }
        }

        /// <summary>
        /// Utility method to quickly find the current application's Notification layer.
        /// </summary>
        /// <param name="element">An element present in the application visual tree.</param>
        /// <returns>The Notification layer container.</returns>
        public static VisualElement FindNotificationLayer(VisualElement element)
        {
            if (element is Panel app)
                return app.notificationContainer;
            return element.GetFirstAncestorOfType<Panel>()?.notificationContainer;
        }

        /// <summary>
        /// Utility method to quickly find the current application's Popup layer.
        /// </summary>
        /// <param name="element">An element present in the application visual tree.</param>
        /// <returns>The Popup layer container.</returns>
        public static VisualElement FindPopupLayer(VisualElement element)
        {
            if (element is Panel app)
                return app.popupContainer;
            return element.GetFirstAncestorOfType<Panel>()?.popupContainer;
        }

        /// <summary>
        /// Utility method to quickly find the current application's Tooltip layer.
        /// </summary>
        /// <param name="element">An element present in the application visual tree.</param>
        /// <returns>The Tooltip layer container.</returns>
        public static VisualElement FindTooltipLayer(VisualElement element)
        {
            if (element is Panel app)
                return app.tooltipContainer;
            return element.GetFirstAncestorOfType<Panel>()?.tooltipContainer;
        }

        static void SetFixedFullScreen(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.top = 0;
            element.style.bottom = 0;
            element.style.left = 0;
            element.style.right = 0;
        }

        /// <summary>
        /// Class used to create instances of <see cref="Panel"/> from UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Panel, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Panel"/>.
        /// </summary>
        public new class UxmlTraits : ContextProvider.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_DefaultTheme = new UxmlStringAttributeDescription
            {
                name = "default-theme",
                defaultValue = "dark"
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
                var app = (Panel)ve;
                app.theme = m_DefaultTheme.GetValueFromBag(bag, cc);
            }
        }

        /// <summary>
        /// Dismiss any open <see cref="Popup"/> that are actually open.
        /// </summary>
        internal void DismissAnyPopups(DismissType reason)
        {
            foreach (var popover in m_DismissablePopups)
            {
                popover?.Dismiss(reason);
            }
            m_DismissablePopups.Clear();
        }

        /// <summary>
        /// Register a <see cref="Popup"/> to the list of dismissable popups.
        /// </summary>
        /// <param name="popup"> The <see cref="Popup"/> to register.</param>
        internal void RegisterPopup(Popup popup)
        {
            m_DismissablePopups.Add(popup);
        }

        /// <summary>
        /// Unregister a <see cref="Popup"/> from the list of dismissable popups.
        /// </summary>
        /// <param name="anchorPopup"></param>
        public void UnregisterPopup(Popup anchorPopup)
        {
            m_DismissablePopups.Remove(anchorPopup);
        }
    }
}
