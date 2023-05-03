using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Dt.App.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Dropdown UI element.
    /// </summary>
    public class Dropdown : ExVisualElement, INotifyValueChanged<int>, ISizeableElement
    {
        /// <summary>
        /// The Dropdown main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-dropdown";

        /// <summary>
        /// The Dropdown title container styling class.
        /// </summary>
        public static readonly string titleContainerUssClassName = ussClassName + "__titlecontainer";

        /// <summary>
        /// The Dropdown title styling class.
        /// </summary>
        public static readonly string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The Dropdown trailing container styling class.
        /// </summary>
        public static readonly string trailingContainerUssClassName = ussClassName + "__trailingcontainer";

        /// <summary>
        /// The Dropdown caret styling class.
        /// </summary>
        public static readonly string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The Dropdown size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Dropdown emphasized mode styling class.
        /// </summary>
        public static readonly string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Dropdown menu styling class.
        /// </summary>
        public static readonly string appuiDropdownMenu = ussClassName + "__menu";

        readonly List<MenuItem> m_Items = new List<MenuItem>();

        readonly LocalizedTextElement m_Title;

        Action<MenuItem, int> m_BindItem;

        int m_DefaultValue = -1;

        Size m_Size;

        IList m_SourceItems;

        int m_Value = -1;

        bool m_ValueSet;

        Clickable m_Clickable;

        MenuBuilder m_MenuBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dropdown()
            : this(null) { }

        /// <summary>
        /// Construct a Dropdown UI element with a provided dynamic collection of items.
        /// </summary>
        /// <param name="items">An items collection.</param>
        /// <param name="bindFunc">The binding function used to populate display data for each item.</param>
        /// <param name="defaultIndex">The selected index by default.</param>
        public Dropdown(IList items, Action<MenuItem, int> bindFunc = null, int defaultIndex = -1)
        {
            AddToClassList(ussClassName);

            clickable = new Submittable(OnClicked);
            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;

            var titleContainer = new VisualElement { name = titleContainerUssClassName, pickingMode = PickingMode.Ignore };
            titleContainer.AddToClassList(titleContainerUssClassName);

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            var trailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            trailingContainer.AddToClassList(trailingContainerUssClassName);

            var caret = new Icon { name = caretUssClassName, iconName = "caret-down", pickingMode = PickingMode.Ignore };
            caret.AddToClassList(caretUssClassName);

            hierarchy.Add(titleContainer);
            titleContainer.hierarchy.Add(m_Title);
            hierarchy.Add(trailingContainer);
            trailingContainer.hierarchy.Add(caret);

            defaultValue = -1;
            size = Size.M;
            sourceItems = items;
            bindItem = bindFunc;
            defaultValue = defaultIndex;

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <summary>
        /// Clickable Manipulator for this Dropdown.
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
        /// A method that is called when a Dropdown item is created and needs to be populated with data.
        /// </summary>
        public Action<MenuItem, int> bindItem
        {
            get => m_BindItem;

            set
            {
                m_BindItem = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// The Dropdown default value. This is the value that will be selected if no value is set.
        /// </summary>
        public int defaultValue
        {
            get => m_DefaultValue;

            set
            {
                m_DefaultValue = value;
                if (!m_ValueSet)
                    SetValueWithoutNotify(m_DefaultValue);
            }
        }

        /// <summary>
        /// The source items collection.
        /// </summary>
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                if (m_SourceItems == value)
                    return;

                m_SourceItems = value;
                m_ValueSet = false;
                RefreshUI();
            }
        }

        /// <summary>
        /// The Dropdown size.
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
        /// The Dropdown emphasized mode.
        /// </summary>
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set => EnableInClassList(emphasizedUssClassName, value);
        }

        /// <summary>
        /// Set the Dropdown value without notifying any listeners.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            m_ValueSet = true;
            m_Value = newValue;
            
#if UNITY_LOCALIZATION_PRESENT
            var selectText = "@AppUI:dropdownSelectMessage";
#else
            var selectText = "Select";
#endif

            // Change dropdown UI
            if (m_Value >= 0 && m_Value < m_Items.Count)
                m_Title.text = m_Items[m_Value].label;
            else
                m_Title.text = selectText;
        }

        /// <summary>
        /// The Dropdown value. This is the index of the selected item.
        /// </summary>
        public int value
        {
            get => m_Value;

            set
            {
                if (value == m_Value)
                    return;

                using var evt = ChangeEvent<int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        void OnClicked()
        {
            m_MenuBuilder?.Dismiss(DismissType.Consecutive);
            if (sourceItems is not { Count: > 0 })
                return;

            m_MenuBuilder = MenuBuilder.Build(this, BuildDropdownMenu());
            AddToClassList(Styles.openUssClassName);
            m_MenuBuilder.dismissed += (_, _) => RemoveFromClassList(Styles.openUssClassName);
            m_MenuBuilder.Show();
        }

        void RefreshUI()
        {
            // clear items
            foreach (var item in m_Items) item.clickable.clickedWithEventInfo -= OnItemClicked;
            m_Items.Clear();

            // create menu items
            if (sourceItems != null)
            {
                for (var i = 0; i < sourceItems.Count; i++)
                {
                    var menuItem = new MenuItem();
                    bindItem?.Invoke(menuItem, i);
                    menuItem.clickable.clickedWithEventInfo += OnItemClicked;
                    m_Items.Add(menuItem);
                }
            }

            SetValueWithoutNotify(m_Value);
        }

        void OnItemClicked(EventBase evt)
        {
            if (evt.target is MenuItem menuItem)
                value = menuItem.parent.IndexOf(menuItem);
        }

        Menu BuildDropdownMenu()
        {
            var menu = new Menu();

            menu.AddToClassList(appuiDropdownMenu);
            menu.style.minWidth = paddingRect.width - 6;

            for (var i = 0; i < m_Items.Count; i++)
            {
                m_Items[i].EnableInClassList(Styles.selectedUssClassName, i == value);
                menu.Add(m_Items[i]);
            }

            return menu;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Dropdown"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Dropdown, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Dropdown"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlIntAttributeDescription m_DefaultValue = new UxmlIntAttributeDescription
            {
                name = "default-value",
                defaultValue = -1,
            };

            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Emphasized = new UxmlBoolAttributeDescription
            {
                name = "emphasized",
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

                var el = (Dropdown)ve;
                el.size = m_Size.GetValueFromBag(bag, cc);
                el.emphasized = m_Emphasized.GetValueFromBag(bag, cc);
                el.defaultValue = m_DefaultValue.GetValueFromBag(bag, cc);
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
