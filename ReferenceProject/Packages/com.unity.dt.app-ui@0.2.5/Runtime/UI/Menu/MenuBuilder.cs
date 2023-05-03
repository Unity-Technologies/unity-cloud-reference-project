using System;
using System.Collections.Generic;
using UnityEngine.Dt.App.Core;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A utility class to build a Menu programmatically.
    /// </summary>
    public class MenuBuilder : AnchorPopup<MenuBuilder>
    {
        readonly Stack<Menu> m_MenuStack;

        IVisualElementScheduledItem m_ScheduledItem;

        const string k_MenuPopoverUssClassName = "appui-popover--menu";

        Popover.PopoverVisualElement popover => (Popover.PopoverVisualElement)view;

        /// <summary>
        /// The last menu in the stack.
        /// </summary>
        public Menu currentMenu => m_MenuStack.Peek();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentView"> The parent view of the Menu. </param>
        /// <param name="context"> The application context. </param>
        /// <param name="view"> The Menu visual element (used by the popup system). </param>
        /// <param name="contentView"> The Menu's content visual element. </param>
        public MenuBuilder(VisualElement parentView, ApplicationContext context, VisualElement view, VisualElement contentView)
            : base(parentView, context, view, contentView)
        {
            m_MenuStack = new Stack<Menu>();
            m_MenuStack.Push((Menu)contentView);

            parentView.panel.visualTree.RegisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
        }

        void OnTreeDown(PointerDownEvent evt)
        {
            if (!outsideClickDismissEnabled)
                return;

            var insideMenus = false;
            foreach (var child in popover.hierarchy.Children())
            {
                if (child.worldBound.Contains((Vector2)evt.position))
                {
                    insideMenus = true;
                    break;
                }
            }
            
            if (insideMenus)
                return;

            var insideAnchor = anchor?.worldBound.Contains((Vector2)evt.position) ?? false;
            var insideLastFocusedElement = (m_LastFocusedElement as VisualElement)?.worldBound.Contains((Vector2)evt.position) ?? false;
            if (insideAnchor || insideLastFocusedElement)
            {
                // prevent reopening the same popover again...
                evt.PreventDefault();
                evt.StopImmediatePropagation();
            }

            Dismiss(DismissType.OutOfBounds);
        }

        /// <summary>
        /// Add an Action menu item to the current menu.
        /// </summary>
        /// <param name="actionId"> A unique identifier for the action. </param>
        /// <param name="labelStr"> The raw label of the menu item (will be localized). </param>
        /// <param name="iconName"> The icon of the menu item. </param>
        /// <param name="callback"> The callback to invoke when the menu item is clicked. </param>
        /// <returns> The MenuBuilder instance. </returns>
        public MenuBuilder AddAction(int actionId, string labelStr, string iconName, EventCallback<ClickEvent> callback)
        {
            var item = new MenuItem
            {
                label = labelStr,
                icon = iconName,
                userData = actionId,
            };
            item.RegisterCallback(callback);
            currentMenu.Add(item);
            return this;
        }

        /// <summary>
        /// Create an action menu item, add a sub-menu to the current menu, and make it the current menu.
        /// </summary>
        /// <param name="actionId"> A unique identifier for the action. </param>
        /// <param name="labelStr"> The label of the menu item. </param>
        /// <param name="iconName"> The icon of the menu item. </param>
        /// <returns> The MenuBuilder instance. </returns>
        public MenuBuilder PushSubMenu(int actionId, string labelStr, string iconName)
        {
            var subMenu = new Menu();
            var item = new MenuItem
            {
                label = labelStr,
                icon = iconName,
                userData = actionId,
                subMenu = subMenu,
            };
            currentMenu.Add(item);
            m_MenuStack.Push(subMenu);
            return this;
        }

        /// <summary>
        /// Go back to the parent menu and make it the current menu.
        /// </summary>
        /// <returns> The MenuBuilder instance. </returns>
        public MenuBuilder Pop()
        {
            m_MenuStack.Pop();
            return this;
        }

        /// <inheritdoc cref="Popup.ShouldAnimate"/>
        protected override bool ShouldAnimate()
        {
            return true;
        }

        /// <inheritdoc cref="Popup.GetFocusableElement"/>
        protected override VisualElement GetFocusableElement()
        {
            return popover.popoverElement;
        }

        /// <inheritdoc cref="AnchorPopup{T}.GetMovableElement"/>
        protected override VisualElement GetMovableElement()
        {
            return popover.popoverElement;
        }

        /// <inheritdoc cref="Popup{T}.InvokeDismissedEventHandlers"/>
        protected override void InvokeDismissedEventHandlers(DismissType reason)
        {
            base.InvokeDismissedEventHandlers(reason);
            targetParent.panel.visualTree.UnregisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
        }

        /// <summary>
        ///  Create a MenuBuilder instance.
        /// </summary>
        /// <param name="referenceView"> The reference view to position the menu. </param>
        /// <param name="menu"> The menu to display. </param>
        /// <returns> The MenuBuilder instance. </returns>
        public static MenuBuilder Build(VisualElement referenceView, Menu menu = null)
        {
            var context = referenceView.GetContext();
            var parentView = context.panel.popupContainer;
            menu = menu ?? new Menu();
            var popoverVisualElement = CreateMenuPopoverVisualElement(menu);
            var menuBuilder = new MenuBuilder(parentView, context, popoverVisualElement, menu)
                .SetLastFocusedElement(referenceView)
                .SetArrowVisible(false)
                .SetPlacement(PopoverPlacement.BottomStart)
                .SetCrossOffset(-8)
                .SetAnchor(referenceView);
            popoverVisualElement.RegisterCallback<ActionTriggeredEvent>(evt =>
            {
                if (evt.target is MenuItem)
                    menuBuilder.Dismiss(DismissType.Action);
            });
            menuBuilder.dismissed += (_, _) => menu.CloseSubMenus();

            return menuBuilder;
        }

        internal static Popover.PopoverVisualElement CreateMenuPopoverVisualElement(Menu menu)
        {
            var popoverVisualElement = new Popover.PopoverVisualElement(menu);
            popoverVisualElement.AddToClassList(k_MenuPopoverUssClassName);
            popoverVisualElement.AddToClassList(Styles.noArrowUssClassName);
            return popoverVisualElement;
        }
    }
}
