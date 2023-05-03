using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// Direction of a UI container. This is used on <see cref="Tabs"/> UI elements for example.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Container's items are stacked horizontally.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Container's items are stacked vertically.
        /// </summary>
        Vertical
    }

    /// <summary>
    /// An item used in <see cref="Tabs"/> bar.
    /// </summary>
    public class TabItem : VisualElement, ISelectableElement, IClickable
    {
        /// <summary>
        /// The TabItem main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-tabitem";

        /// <summary>
        /// The TabItem label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The TabItem icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_Label;

        Clickable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TabItem()
        {
            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            clickable = new Submittable((Action)null);

            AddToClassList(ussClassName);

            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            label = null;
            icon = null;
            selected = false;
        }

        /// <summary>
        /// The TabItem label.
        /// </summary>
        public string label
        {
            get => m_Label.text;
            set
            {
                m_Label.text = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Label.text));
            }
        }

        /// <summary>
        /// The TabItem icon.
        /// </summary>
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                m_Icon.iconName = value;
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Icon.iconName));
            }
        }

        /// <summary>
        /// Clickable Manipulator for this TabItem.
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
        /// The selected state of the TabItem.
        /// </summary>
        public bool selected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set => SetSelectedWithoutNotify(value);
        }

        /// <summary>
        /// Set the selected state of the TabItem without notifying the selection system.
        /// </summary>
        /// <param name="newValue"> The new selected state.</param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.selectedUssClassName, newValue);
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="TabItem"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<TabItem, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="TabItem"/>.
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

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
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

                var el = (TabItem)ve;
                el.icon = m_Icon.GetValueFromBag(bag, cc);
                el.label = m_Label.GetValueFromBag(bag, cc);

                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }

    /// <summary>
    /// Tabs UI element.
    /// </summary>
    public class Tabs : VisualElement, INotifyValueChanged<int>
    {
        /// <summary>
        /// The Tabs main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-tabs";

        /// <summary>
        /// The Tabs size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Tabs direction styling class.
        /// </summary>
        public static readonly string orientationUssClassName = ussClassName + "--";

        /// <summary>
        /// The Tabs emphasized mode styling class.
        /// </summary>
        public static readonly string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Tabs container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Tabs indicator styling class.
        /// </summary>
        public static readonly string indicatorUssClassName = ussClassName + "__indicator";

        readonly VisualElement m_Indicator;

        readonly List<TabItem> m_Items = new List<TabItem>();

        readonly ScrollView m_ScrollView;

        Action<TabItem, int> m_BindItem;

        int m_DefaultValue;

        Direction m_Direction;

        Size m_Size;

        IList m_SourceItems;

        int m_Value;

        bool m_ValueSet;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Tabs()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_ScrollView = new ScrollView
            {
                name = containerUssClassName,
#if (UNITY_2021_3 && UNITY_2021_3_NIK) || (UNITY_2022_1 && UNITY_2022_1_NIK) || (UNITY_2022_2 && UNITY_2022_2_NIK) || UNITY_2022_3 || (UNITY_2023_1 && UNITY_2023_1_NIK) || UNITY_2023_2_OR_NEWER
                nestedInteractionKind = ScrollView.NestedInteractionKind.StopScrolling,
#endif
                mode = ScrollViewMode.Horizontal,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            m_ScrollView.AddToClassList(containerUssClassName);
            m_Indicator = new VisualElement
            {
                name = indicatorUssClassName,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_Indicator.AddToClassList(indicatorUssClassName);

            hierarchy.Add(m_ScrollView);
            hierarchy.Add(m_Indicator);

            size = Size.M;
            emphasized = false;
            direction = Direction.Horizontal;
            defaultValue = 0;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_ScrollView.verticalScroller.valueChanged += OnVerticalScrollerChanged;
            m_ScrollView.horizontalScroller.valueChanged += OnHorizontalScrollerChanged;
        }

        /// <summary>
        /// The size of the Tabs.
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
        /// The direction of the Tabs. Horizontal or Vertical.
        /// </summary>
        public Direction direction
        {
            get => m_Direction;
            set
            {
                RemoveFromClassList(orientationUssClassName + m_Direction.ToString().ToLower());
                m_Direction = value;
                AddToClassList(orientationUssClassName + m_Direction.ToString().ToLower());
                m_ScrollView.mode = m_Direction switch
                {
                    Direction.Vertical => ScrollViewMode.Vertical,
                    _ => ScrollViewMode.Horizontal
                };
            }
        }

        /// <summary>
        /// The emphasized mode of the Tabs.
        /// </summary>
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set => EnableInClassList(emphasizedUssClassName, value);
        }

        /// <summary>
        /// Method to bind the TabItem.
        /// </summary>
        public Action<TabItem, int> bindItem
        {
            get => m_BindItem;

            set
            {
                m_BindItem = value;
                RefreshItems(sourceItems);
            }
        }

        /// <summary>
        /// Collection of items used to populate the Tabs.
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
                RefreshItems(sourceItems);
            }
        }

        /// <summary>
        /// The content container of the Tabs.
        /// </summary>
        public override VisualElement contentContainer => m_ScrollView.contentContainer;

        /// <summary>
        /// The default value of the Tabs. This is the value that will be selected if no value is set.
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
        /// Set the value of the Tabs without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value.</param>
        /// <exception cref="ValueOutOfRangeException"> Throws if the value is out of range.</exception>
        public void SetValueWithoutNotify(int newValue)
        {
            //check the state of the target element
            if (m_Value >= 0 && m_Value < m_Items.Count && !m_Items[m_Value].enabledSelf)
                return;

            var previousValue = m_Value;
            m_Value = newValue;
            m_ValueSet = true;

            // refresh selection visually
            if (previousValue >= 0 && previousValue < m_Items.Count)
                m_Items[previousValue].selected = false;
            if (m_Value >= 0 && m_Value < m_Items.Count)
            {
                m_Items[m_Value].selected = true;
                m_ScrollView.ScrollTo(m_Items[m_Value]);
                switch (direction)
                {
                    case Direction.Horizontal:
                        m_Indicator.style.width = m_Items[m_Value].worldBound.width;
                        m_Indicator.style.left = m_Items[m_Value].localBound.x - m_ScrollView.scrollOffset.x;
                        m_Indicator.style.top = StyleKeyword.Null;
                        m_Indicator.style.height = StyleKeyword.Null;
                        break;
                    case Direction.Vertical:
                        m_Indicator.style.height = m_Items[m_Value].worldBound.height;
                        m_Indicator.style.top = m_Items[m_Value].localBound.y - m_ScrollView.scrollOffset.y;
                        m_Indicator.style.width = StyleKeyword.Null;
                        m_Indicator.style.left = StyleKeyword.Null;
                        break;
                    default:
                        throw new ValueOutOfRangeException(nameof(direction), direction);
                }
            }
            else
            {
                switch (direction)
                {
                    case Direction.Horizontal:
                        m_Indicator.style.width = 0;
                        m_Indicator.style.left = 0;
                        m_Indicator.style.top = StyleKeyword.Null;
                        m_Indicator.style.height = StyleKeyword.Null;
                        break;
                    case Direction.Vertical:
                        m_Indicator.style.height = 0;
                        m_Indicator.style.top = 0;
                        m_Indicator.style.width = StyleKeyword.Null;
                        m_Indicator.style.left = StyleKeyword.Null;
                        break;
                    default:
                        throw new ValueOutOfRangeException(nameof(direction), direction);
                }
            }
        }

        /// <summary>
        /// The value of the Tabs. This is the index of the selected TabItem.
        /// </summary>
        public int value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;

                var previousValue = m_Value;
                SetValueWithoutNotify(value);
                if (previousValue != m_Value)
                {
                    using var evt = ChangeEvent<int>.GetPooled(previousValue, m_Value);
                    evt.target = this;
                    SendEvent(evt);
                }
            }
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;

            if (direction == Direction.Horizontal)
            {
                if (evt.keyCode == KeyCode.LeftArrow)
                    handled = GoToPrevious();
                else if (evt.keyCode == KeyCode.RightArrow) handled = GoToNext();
            }
            else
            {
                if (evt.keyCode == KeyCode.UpArrow)
                    handled = GoToPrevious();
                else if (evt.keyCode == KeyCode.DownArrow) handled = GoToNext();
            }

            if (handled)
            {
                evt.PreventDefault();
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// Go to the next TabItem.
        /// </summary>
        /// <returns> True if the next TabItem is selected, false otherwise.</returns>
        public bool GoToNext()
        {
            var nextIndex = Mathf.Clamp(value + 1, 0, childCount - 1);
            while (!ElementAt(nextIndex).enabledSelf) nextIndex = Mathf.Clamp(nextIndex + 1, 0, childCount - 1);
            if (nextIndex >= childCount || nextIndex == value)
                return false;
            value = nextIndex;
            return true;
        }

        /// <summary>
        /// Go to the previous TabItem.
        /// </summary>
        /// <returns> True if the previous TabItem is selected, false otherwise.</returns>
        public bool GoToPrevious()
        {
            var nextIndex = Mathf.Clamp(value - 1, 0, childCount - 1);
            while (!ElementAt(nextIndex).enabledSelf) nextIndex = Mathf.Clamp(nextIndex - 1, 0, childCount - 1);
            if (nextIndex == value || nextIndex < 0)
                return false;
            value = nextIndex;
            return true;
        }

        void OnHorizontalScrollerChanged(float offset)
        {
            if (direction == Direction.Horizontal)
                SetValueWithoutNotify(value);
        }

        void OnVerticalScrollerChanged(float offset)
        {
            if (direction == Direction.Vertical)
                SetValueWithoutNotify(value);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (childCount > 0 && sourceItems == null)
                RefreshItems(Children());
        }

        void RefreshItems(IEnumerable items)
        {
            // clear items
            foreach (var item in m_Items) item.clickable.clickedWithEventInfo -= OnItemClicked;
            m_Items.Clear();

            // create menu items
            if (items != null)
            {
                var i = 0;
                foreach (var item in items)
                {
                    var tabItem = item as TabItem ?? new TabItem();
                    bindItem?.Invoke(tabItem, i);
                    tabItem.clickable.clickedWithEventInfo += OnItemClicked;
                    m_Items.Add(tabItem);
                    i++;
                }
            }

            SetValueWithoutNotify(m_Value);
        }

        void OnItemClicked(EventBase evt)
        {
            if (evt.target is TabItem item)
                value = item.parent.IndexOf(item);
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Tabs"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Tabs, UxmlTraits>
        {
            /// <summary>
            /// Describes the types of element that can appear as children of this element in a UXML file.
            /// </summary>
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription => new[]
            {
                new UxmlChildElementDescription(typeof(TabItem))
            };
        }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="Tabs"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlIntAttributeDescription m_DefaultValue = new UxmlIntAttributeDescription
            {
                name = "default-value",
                defaultValue = 0
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

            readonly UxmlEnumAttributeDescription<Direction> m_Orientation = new UxmlEnumAttributeDescription<Direction>
            {
                name = "direction",
                defaultValue = Direction.Horizontal,
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
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);
                var el = (Tabs)ve;
                el.size = m_Size.GetValueFromBag(bag, cc);
                el.direction = m_Orientation.GetValueFromBag(bag, cc);
                el.emphasized = m_Emphasized.GetValueFromBag(bag, cc);
                el.defaultValue = m_DefaultValue.GetValueFromBag(bag, cc);

                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
