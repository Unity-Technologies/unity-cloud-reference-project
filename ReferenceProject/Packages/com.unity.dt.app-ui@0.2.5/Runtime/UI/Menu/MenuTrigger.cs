using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A wrapper to display a menu when a trigger has been activated.
    /// </summary>
    public class MenuTrigger : VisualElement
    {
        string m_AnchorName;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuTrigger()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// The trigger used to determine when to display them <see cref="menu"/>.
        /// </summary>
        public VisualElement trigger { get; private set; }

        /// <summary>
        /// The UI element used as an anchor for the menu's popover.
        /// </summary>
        public VisualElement anchor { get; set; }

        /// <summary>
        /// The menu to display.
        /// </summary>
        public Menu menu { get; private set; }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Menu childMenu = null;
            VisualElement ve = null;

            foreach (var child in Children())
            {
                if (childMenu == null && child is Menu m)
                    childMenu = m;
                
                if (ve == null && !(child is Menu))
                    ve = child;
                
                if (childMenu != null && ve != null)
                    break;
            }
            
            if (childMenu != null && childMenu != menu)
            {
                // New Dialog attached as child
                menu = childMenu;
                Remove(childMenu);
            }

            if (ve != null && ve != trigger)
            {
                if (trigger is IClickable c1)
                    c1.clickable.clicked -= OnActionTriggered;
                trigger = ve;
                if (trigger is IClickable c2)
                    c2.clickable.clicked += OnActionTriggered;
            }

            // we can also try to find the anchor (if any has been given with the UXML attribute)
            if (!string.IsNullOrEmpty(m_AnchorName) && panel != null)
            {
                var anchorElement = panel.visualTree.Q<VisualElement>(m_AnchorName);
                if (anchorElement != null)
                    anchor = anchorElement;
                else
                    Debug.LogWarning($"Unable to find {m_AnchorName}");
            }
        }

        void OnActionTriggered()
        {
            var popover = MenuBuilder.Build(anchor ?? trigger, menu);
            popover.Show();
        }

        /// <summary>
        /// UXML factory for the <see cref="MenuTrigger"/>.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<MenuTrigger, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="MenuTrigger"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Anchor = new UxmlStringAttributeDescription
            {
                name = "anchor",
                defaultValue = null
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

                var el = (MenuTrigger)ve;

                el.m_AnchorName = m_Anchor.GetValueFromBag(bag, cc);
            }
        }
    }
}
