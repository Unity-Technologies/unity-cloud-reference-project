using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Menu UI element.
    /// </summary>
    public class Menu : VisualElement
    {
        /// <summary>
        /// The Menu main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-menu";

        /// <summary>
        /// The Menu container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Menu selectable mode styling class.
        /// </summary>
        public static readonly string selectableUssClassName = ussClassName + "--selectable";

        readonly ScrollView m_ScrollView;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Menu()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_ScrollView = new ScrollView
            {
                name = containerUssClassName,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            m_ScrollView.AddToClassList(containerUssClassName);
            hierarchy.Add(m_ScrollView);
        }

        /// <summary>
        /// The content container of the Menu.
        /// </summary>
        public override VisualElement contentContainer => m_ScrollView.contentContainer;

        /// <summary>
        /// The parent MenuItem of this Menu (if this Menu is a sub-menu).
        /// </summary>
        public MenuItem parentItem { get; internal set; } = null;

        /// <summary>
        /// Executes logic after the callbacks registered on the event target have executed, unless the event is marked to prevent its default behaviour. EventBase{T}.PreventDefault.
        /// </summary>
        /// <param name="evt"> The event to execute the default action on. </param>
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);

            if (evt.eventTypeId == GeometryChangedEvent.TypeId())
            {
                if (panel != null)
                {
                    var query = this.Query<MenuItem>().Where(item => item.selectable).Build();
                    EnableInClassList(selectableUssClassName, query.ToList().Count > 0);
                }
            }
        }

        /// <summary>
        /// Close all sub-menus.
        /// </summary>
        public void CloseSubMenus()
        {
            foreach (var menuItem in this.GetChildren<MenuItem>(false))
            {
                if (menuItem.subMenu != null)
                    menuItem.CloseSubMenus(Vector2.negativeInfinity, menuItem.subMenu);
            }
        }

        /// <summary>
        /// Get all the MenuItems of this Menu (including sub-menus).
        /// </summary>
        /// <returns> The list of MenuItems. </returns>
        public IEnumerable<MenuItem> GetMenuItems()
        {
            return this.GetChildren<MenuItem>(true);
        }

        /// <summary>
        /// Class to be able to instantiate a <see cref="Menu"/> from UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Menu, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Menu"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
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
            }
        }
    }
}
