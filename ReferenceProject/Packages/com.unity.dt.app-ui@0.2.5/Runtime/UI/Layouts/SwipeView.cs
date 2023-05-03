using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A SwipeViewItem is an item that must be used as a child of a <see cref="SwipeView"/>.
    /// </summary>
    public class SwipeViewItem : VisualElement
    {
        /// <summary>
        /// The main styling class of the SwipeViewItem. This is the class that is used in the USS file.
        /// </summary>
        public static readonly string ussClassName = "appui-swipeview-item";

        /// <summary>
        /// The index of the item in the SwipeView.
        /// </summary>
        public int index { get; internal set; }

        /// <summary>
        /// The SwipeView that contains this item.
        /// </summary>
        public SwipeView view => parent as SwipeView;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SwipeViewItem()
        {
            AddToClassList(ussClassName);

            usageHints = UsageHints.DynamicTransform;
        }

        /// <summary>
        /// Defines the UxmlFactory for the SwipeViewItem.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SwipeViewItem, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SwipeViewItem"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {

        }
    }

    /// <summary>
    /// A SwipeView is a container that displays one or more children at a time and provides a UI to
    /// navigate between them. It is similar to a <see cref="ScrollView"/> but here children are
    /// snapped to the container's edges. See <see cref="PageView"/> for a similar container that
    /// includes a page indicator.
    /// </summary>
    public class SwipeView : VisualElement, INotifyValueChanged<int>
    {
        /// <summary>
        /// The main styling class of the SwipeView. This is the class that is used in the USS file.
        /// </summary>
        public static readonly string ussClassName = "appui-swipeview";

        /// <summary>
        /// The styling class applied to the container of the SwipeView.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The styling class applied to the SwipeView depending on its orientation.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";

        static readonly PropertyInfo k_Recycled = typeof(ValueAnimation<float>).GetProperty("recycled", BindingFlags.Instance | BindingFlags.NonPublic);

        bool m_Wrap;

        List<SwipeViewItem> m_StaticItems;

        Direction m_Direction;

        int m_Value = -1;

        ValueAnimation<float> m_Animation;

        int m_VisibleItemCount;

        IVisualElementScheduledItem m_PollHierarchyItem;

        IList m_SourceItems;

        readonly VisualElement m_Container;

        bool m_ForceDisableWrap;

        bool m_GoingPrevious;

        bool m_GoingNext;
        
        readonly Scrollable m_Scrollable;

        /// <summary>
        /// The container of the SwipeView.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The speed of the animation when snapping to an item.
        /// </summary>
        public float snapAnimationSpeed { get; set; } = 0.5f;
        
        /// <summary>
        /// The easing of the animation when snapping to an item.
        /// </summary>
        public Func<float, float> snapAnimationEasing { get; set; } = Easing.OutCubic;
        
        /// <summary>
        /// The amount of pixels that must be swiped before the SwipeView begins to swipe.
        /// </summary>
        public float startSwipeThreshold
        {
            get => m_Scrollable.threshold;
            set => m_Scrollable.threshold = value;
        }

        /// <summary>
        /// The number of items that are visible at the same time.
        /// </summary>
        public int visibleItemCount
        {
            get => m_VisibleItemCount;
            set
            {
                m_VisibleItemCount = value;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// The orientation of the SwipeView.
        /// </summary>
        public Direction direction
        {
            get => m_Direction;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Direction.ToString().ToLower());
                m_Direction = value;
                AddToClassList(variantUssClassName + m_Direction.ToString().ToLower());
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// A method that is called when an item is bound to the SwipeView.
        /// </summary>
        public Action<SwipeViewItem, int> bindItem { get; set; }

        /// <summary>
        /// A method that is called when an item is unbound from the SwipeView.
        /// </summary>
        public Action<SwipeViewItem, int> unbindItem { get; set; }

        /// <summary>
        /// The source of items that are used to populate the SwipeView.
        /// </summary>
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                m_SourceItems = value;

                // Stop Polling the hierarchy as we provided a new set of items
                m_PollHierarchyItem?.Pause();
                m_PollHierarchyItem = null;

                RefreshList();
            }
        }

        /// <summary>
        /// This property determines whether or not the view wraps around when it reaches the start or end.
        /// </summary>
        public bool wrap
        {
            get => m_Wrap;
            set
            {
                m_Wrap = value;
                RefreshList();
            }
        }

        /// <summary>
        /// The total number of items.
        /// </summary>
        public int count => items?.Count ?? 0;

        /// <summary>
        /// Determine if the SwipeView should wrap around.
        /// </summary>
        internal bool shouldWrap => count > visibleItemCount && wrap && !m_ForceDisableWrap;

        /// <summary>
        /// The current item.
        /// </summary>
        public SwipeViewItem currentItem => GetItem(value);

        IList items => m_SourceItems ?? m_StaticItems;

        SwipeViewItem GetItem(int index)
        {
            foreach (var child in Children())
            {
                if (child is SwipeViewItem item && item.index == index)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// The event that is called when the value of the SwipeView changes (i.e. when its being swiped or when it snaps to an item).
        /// </summary>
        public event Action<SwipeViewItem, float> beingSwiped;

        /// <summary>
        /// The value of the SwipeView (i.e. the index of the current item).
        /// </summary>
        public int value
        {
            get => m_Value;
            set
            {
                if (count == 0)
                    return;

                if (value < 0 || value > count - 1)
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

        /// <summary>
        /// This property determines the threshold at which the animation will be skipped.
        /// </summary>
        public int skipAnimationThreshold { get; set; } = 2;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SwipeView()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            usageHints = UsageHints.GroupTransform;

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            visibleItemCount = 1;

            m_PollHierarchyItem = schedule.Execute(PollHierarchy).Every(50L);

            m_Scrollable = new Scrollable(OnDrag, OnUp);
            this.AddManipulator(m_Scrollable);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_Container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid())
                return;

            RefreshItemsSize();
            SetValueWithoutNotify(value);
            InvokeSwipeEvents();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;
            if (evt.target == this)
            {
                if (direction == Direction.Horizontal)
                {
                    if (evt.keyCode == KeyCode.LeftArrow)
                        handled = GoToPrevious();
                    else if (evt.keyCode == KeyCode.RightArrow)
                        handled = GoToNext();
                }
                else
                {
                    if (evt.keyCode == KeyCode.UpArrow)
                        handled = GoToPrevious();
                    else if (evt.keyCode == KeyCode.DownArrow)
                        handled = GoToNext();
                }
            }

            if (handled)
            {
                evt.PreventDefault();
                evt.StopPropagation();
            }
        }

        void OnUp(Scrollable draggable)
        {
            m_ForceDisableWrap = true;
            if (draggable.hasMoved)
            {
                value = GetClosestIndex();
            }
            else
            {
                var pos = this.LocalToWorld(draggable.localPosition);
                VisualElement hoveredChild = null;
                foreach (var child in Children())
                {
                    if (child.ContainsPoint(child.WorldToLocal(pos)))
                    {
                        hoveredChild = child;
                        break;
                    }
                }
                if (hoveredChild != null)
                    value = ((SwipeViewItem)hoveredChild).index;
            }
            m_ForceDisableWrap = false;
        }

        void OnDrag(Scrollable drag)
        {
            if (m_Animation != null && !((bool)k_Recycled!.GetValue(m_Animation)))
                m_Animation?.Recycle();

            if (direction == Direction.Horizontal)
                m_Container.style.left = m_Container.resolvedStyle.left + drag.deltaPos.x;
            else
                m_Container.style.top = m_Container.resolvedStyle.top + drag.deltaPos.y;
        }

        SwipeViewItem GetClosestElement()
        {
            if (items == null || count <= 0)
                return null;

            var best = (SwipeViewItem)ElementAt(0);
            var center = this.WorldToLocal(best.worldBound.min);
            var bestDistance = Mathf.Abs(direction == Direction.Horizontal ? center.x : center.y);

            for (var i = 1; i < childCount; i++)
            {
                var candidate = (SwipeViewItem)ElementAt(i);
                center = this.WorldToLocal(candidate.worldBound.min);
                var candidateDistance = Mathf.Abs(direction == Direction.Horizontal ? center.x : center.y);
                if (candidateDistance < bestDistance)
                {
                    bestDistance = candidateDistance;
                    best = candidate;
                }
            }

            return best;
        }

        int GetClosestIndex()
        {
            return GetClosestElement()?.index ?? -1; ;
        }

        bool IsItemCurrentlyVisible(VisualElement c)
        {
            var rect = this.WorldToLocal(c.worldBound);
            return direction == Direction.Horizontal
                ? (rect.x >= 0 && rect.x < localBound.width) || (rect.xMax > 0 && rect.xMax <= localBound.width)
                : (rect.y >= 0 && rect.y < localBound.height) || (rect.yMax > 0 && rect.yMax <= localBound.height);
        }

        /// <summary>
        /// Sets the value without notifying the listeners.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public void SetValueWithoutNotify(int newValue)
        {
            if (count == 0)
            {
                m_Value = -1;
                return;
            }

            if (newValue < 0 || newValue > count - 1)
                return;

            if (shouldWrap)
            {
                var currentElementIndex = IndexOf(currentItem);
                var nextElementIndex = IndexOf(GetItem(newValue));
                if (!m_GoingPrevious && (m_Value < newValue || m_GoingNext) && nextElementIndex < currentElementIndex)
                {
                    var goingNext = m_GoingNext;
                    // the next item is placed before the current one,
                    // move items to the end to get a more pleasant order
                    var itemsToMove = new List<VisualElement>();
                    var children = new List<VisualElement>(Children());
                    var i = 0;
                    while (i < children.Count && !IsItemCurrentlyVisible(children[i]))
                    {
                        itemsToMove.Add(children[i]);
                        i++;
                    }
                    m_Container.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
                    SwapFirstToLast(itemsToMove.Count);
                    schedule.Execute(() =>
                    {
                        m_GoingNext = goingNext;
                        value = newValue;
                        m_GoingNext = false;
                    }).ExecuteLater(16L);
                    return;
                }

                if (!m_GoingNext && (m_Value > newValue || m_GoingPrevious) && nextElementIndex > currentElementIndex)
                {
                    var goingPrevious = m_GoingPrevious;
                    // the previous item is placed after the current one,
                    // move items to the start to get a more pleasant order
                    var itemsToMove = new List<VisualElement>();
                    var children = new List<VisualElement>(Children());
                    var i = children.Count - 1;
                    while (i >= 0 && !IsItemCurrentlyVisible(children[i]))
                    {
                        itemsToMove.Add(children[i]);
                        i--;
                    }
                    m_Container.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
                    SwapLastToFirst(itemsToMove.Count);
                    schedule.Execute(() =>
                    {
                        m_GoingPrevious = goingPrevious;
                        value = newValue;
                        m_GoingPrevious = false;
                    }).ExecuteLater(16L);
                    return;
                }
            }
            else
            {
                newValue = Mathf.Clamp(newValue, 0, count - m_VisibleItemCount);
            }

            var from = m_Value >= 0 ? GetItem(m_Value) : null;
            var to = GetItem(newValue);

            if (paddingRect.IsValid())
                StartSwipeAnimation(from, to);

            from?.RemoveFromClassList(Styles.selectedUssClassName);
            m_Value = newValue;
            to?.AddToClassList(Styles.selectedUssClassName);
        }

        void StartSwipeAnimation(VisualElement from, VisualElement to)
        {
            // Need a valid destination to create the animation
            if (to == null)
                return;

            // Find the position where the container must be at the end of the animation
            var newElementMin = this.WorldToLocal(to.worldBound.min);
            var newElementSize = direction == Direction.Horizontal ? to.worldBound.width : to.worldBound.height;
            var newElementOffset = direction == Direction.Horizontal ? newElementMin.x : newElementMin.y;
            var currentContainerOffset = direction == Direction.Horizontal
                ? m_Container.resolvedStyle.left
                : m_Container.resolvedStyle.top;
            var targetContainerOffset = currentContainerOffset - newElementOffset;

            // Recycle previous animation
            if (m_Animation != null && !((bool)k_Recycled!.GetValue(m_Animation)))
                m_Animation?.Recycle();

            // Find the best duration and distance to use in the animation
            var duration = from == null || newElementOffset == 0
                ? 0
                : Mathf.RoundToInt(Mathf.Abs(newElementOffset) / snapAnimationSpeed);

            // The best distance takes in account the max distance based on skipAnimationThreshold property
            var distance = Mathf.Abs(targetContainerOffset - currentContainerOffset);
            var sign = Mathf.Sign(targetContainerOffset - currentContainerOffset);
            distance = Mathf.Min(distance, skipAnimationThreshold * newElementSize);
            currentContainerOffset = targetContainerOffset - sign * distance;

            // Start the animation
            m_Animation = experimental.animation.Start(currentContainerOffset, targetContainerOffset, duration, (_, f) =>
            {
                if (direction == Direction.Horizontal)
                    m_Container.style.left = f;
                else
                    m_Container.style.top = f;
            }).Ease(snapAnimationEasing).KeepAlive();
        }

        void PollHierarchy()
        {
            if (m_StaticItems == null && childCount > 0 && m_SourceItems == null)
            {
                m_PollHierarchyItem?.Pause();
                m_PollHierarchyItem = null;
                m_StaticItems = new List<SwipeViewItem>();
                foreach (var c in Children())
                {
                    m_StaticItems.Add((SwipeViewItem)c);
                }
                RefreshList();
            }
        }

        void RefreshItemsSize()
        {
            if (!contentRect.IsValid())
                return;

            foreach (var c in Children())
            {
                if (direction == Direction.Horizontal)
                    c.style.width = contentRect.width / m_VisibleItemCount;
                else
                    c.style.height = contentRect.height / m_VisibleItemCount;
            }
        }

        void RefreshList()
        {
            for (var i = 0; i < childCount; i++)
            {
                var item = (SwipeViewItem)ElementAt(i);
                unbindItem?.Invoke(item, i);
            }

            Clear();

            if (m_SourceItems != null)
            {
                for (var i = 0; i < m_SourceItems.Count; i++)
                {
                    var item = new SwipeViewItem { index = i };
                    bindItem?.Invoke(item, i);
                    Add(item);
                }
            }
            else if (m_StaticItems != null)
            {
                for (var i = 0; i < m_StaticItems.Count; i++)
                {
                    var item = new SwipeViewItem { index = i };
                    if (m_StaticItems[i].childCount > 0)
                        item.Add(m_StaticItems[i].ElementAt(0));
                    Add(item);
                }
            }

            RefreshItemsSize();

            if (childCount > 0)
            {
                value = 0;
            }
            else
            {
                m_Value = -1;
            }
        }

        void SwapLastToFirst() => SwapLastToFirst(1);

        void SwapLastToFirst(int times)
        {
            if (direction == Direction.Horizontal)
                m_Container.style.left = m_Container.resolvedStyle.left - contentRect.width * times;
            else
                m_Container.style.top = m_Container.resolvedStyle.top - contentRect.height * times;

            while (times > 0)
            {
                var item = ElementAt(childCount - 1);
                item.SendToBack();
                times--;
            }
            m_Container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        void SwapFirstToLast() => SwapFirstToLast(1);

        void SwapFirstToLast(int times)
        {
            if (direction == Direction.Horizontal)
                m_Container.style.left = m_Container.resolvedStyle.left + contentRect.width * times;
            else
                m_Container.style.top = m_Container.resolvedStyle.top + contentRect.height * times;

            while (times > 0)
            {
                var item = ElementAt(0);
                item.BringToFront();
                times--;
            }

            m_Container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        void InvokeSwipeEvents()
        {
            if (!paddingRect.IsValid() || beingSwiped == null)
                return;

            foreach (var item in Children())
            {
                var size = direction == Direction.Horizontal ? item.localBound.width : item.localBound.height;
                var localRect = this.WorldToLocal(item.worldBound);
                var normalizedDistance = direction == Direction.Horizontal ? localRect.x / size : localRect.y / size;
                beingSwiped?.Invoke((SwipeViewItem)item, normalizedDistance);
            }
        }

        void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid())
                return;

            var containerMin = direction == Direction.Horizontal ? evt.newRect.x : evt.newRect.y;
            var containerMax = direction == Direction.Horizontal ? evt.newRect.xMax : evt.newRect.yMax;

            switch (shouldWrap)
            {
                case true when containerMin > 0:
                    m_Container.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
                    schedule.Execute(SwapLastToFirst).ExecuteLater(16L);
                    break;
                case true when containerMax < paddingRect.width:
                    m_Container.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
                    schedule.Execute(SwapFirstToLast).ExecuteLater(16L);
                    break;
            }

            InvokeSwipeEvents();
        }

        /// <summary>
        /// Check if there is a next item or not.
        /// </summary>
        public bool canGoToNext => shouldWrap || (value + 1 < childCount && value + 1 >= 0);

        /// <summary>
        /// Check if there is a previous item or not.
        /// </summary>
        public bool canGoToPrevious => shouldWrap || (value - 1 < childCount && value - 1 >= 0);
        
        /// <summary>
        /// Go to item at index.
        /// </summary>
        /// <param name="index"> Index of the item to go to. </param>
        /// <returns> True if the operation was successful, false otherwise. </returns>
        public bool GoTo(int index)
        {
            if (index < 0 || index >= childCount)
                return false;

            value = index;
            return true;
        }

        /// <summary>
        /// Snap to item at index.
        /// </summary>
        /// <param name="index"> Index of the item to snap to. </param>
        /// <returns> True if the operation was successful, false otherwise. </returns>
        public bool SnapTo(int index)
        {
            var skipAnimation = skipAnimationThreshold;
            skipAnimationThreshold = 0;
            var result = GoTo(index);
            skipAnimationThreshold = skipAnimation;
            return result;
        }

        /// <summary>
        /// Go to next item.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool GoToNext()
        {
            if (!canGoToNext)
                return false;

            var nextIndex = shouldWrap
                ? (int)Mathf.Repeat(value + 1, childCount)
                : Mathf.Clamp(value + 1, 0, childCount - visibleItemCount);

            if (nextIndex == value)
                return false;

            m_GoingNext = true;
            value = nextIndex;
            m_GoingNext = false;

            return true;
        }

        /// <summary>
        /// Go to previous item.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool GoToPrevious()
        {
            if (!canGoToPrevious)
                return false;

            var nextIndex = shouldWrap
                ? (int)Mathf.Repeat(value - 1, childCount)
                : Mathf.Clamp(value - 1, 0, childCount - visibleItemCount);

            if (nextIndex == value)
                return false;

            m_GoingPrevious = true;
            value = nextIndex;
            m_GoingPrevious = false;

            return true;
        }

        /// <summary>
        /// Defines the UxmlFactory for the SwipeView.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SwipeView, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SwipeView"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlEnumAttributeDescription<Direction> m_Direction = new UxmlEnumAttributeDescription<Direction>()
            {
                name = "direction",
                defaultValue = Direction.Horizontal,
            };

            readonly UxmlFloatAttributeDescription m_AnimationSpeed = new UxmlFloatAttributeDescription()
            {
                name = "animation-speed",
                defaultValue = 0.5f,
            };

            readonly UxmlIntAttributeDescription m_SkipAnim = new UxmlIntAttributeDescription()
            {
                name = "skip-animation-threshold",
                defaultValue = 2,
            };

            readonly UxmlBoolAttributeDescription m_Wrap = new UxmlBoolAttributeDescription()
            {
                name = "wrap",
                defaultValue = false,
            };

            readonly UxmlIntAttributeDescription m_VisibleItemCount = new UxmlIntAttributeDescription()
            {
                name = "visible-item-count",
                defaultValue = 1,
            };
            
            readonly UxmlFloatAttributeDescription m_StartSwipeThreshold = new UxmlFloatAttributeDescription()
            {
                name = "start-swipe-threshold",
                defaultValue = 24f,
            };

            /// <summary>
            /// Returns an enumerable containing UxmlChildElementDescription(typeof(VisualElement)), since VisualElements can contain other VisualElements.
            /// </summary>
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription =>
                new UxmlChildElementDescription[]
                {
                    new UxmlChildElementDescription(typeof(SwipeViewItem))
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

                var el = (SwipeView)ve;
                el.direction = m_Direction.GetValueFromBag(bag, cc);
                el.wrap = m_Wrap.GetValueFromBag(bag, cc);
                el.visibleItemCount = m_VisibleItemCount.GetValueFromBag(bag, cc);
                el.skipAnimationThreshold = m_SkipAnim.GetValueFromBag(bag, cc);
                el.snapAnimationSpeed = m_AnimationSpeed.GetValueFromBag(bag, cc);
                el.startSwipeThreshold = m_StartSwipeThreshold.GetValueFromBag(bag, cc);
            }
        }
    }
}
