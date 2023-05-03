using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// The status of a StackViewItem.
    /// </summary>
    public enum StackViewItemStatus
    {
        /// <summary>
        /// The item is not in the stack.
        /// </summary>
        Inactive,
        /// <summary>
        /// The item is in the process of being deactivated.
        /// </summary>
        Deactivating,
        /// <summary>
        /// The item is in the process of being activated.
        /// </summary>
        Activating,
        /// <summary>
        /// The item is active.
        /// </summary>
        Active,
    }

    /// <summary>
    /// The operation to perform on a StackView.
    /// </summary>
    public enum StackViewOperation
    {
        /// <summary>
        /// Unused.
        /// </summary>
        Immediate,
        /// <summary>
        /// Push a new item in the stack.
        /// </summary>
        PushTransition,
        /// <summary>
        /// Replace the current item in the stack.
        /// </summary>
        ReplaceTransition,
        /// <summary>
        /// Pop the current item in the stack.
        /// </summary>
        PopTransition,
    }

    /// <summary>
    /// An item in a StackView. It is a container for any UI element and must be used inside a StackView.
    /// </summary>
    public class StackViewItem : VisualElement
    {
        /// <summary>
        /// The main styling class of the StackViewItem. This is the class that is used in the USS file.
        /// </summary>
        public static readonly string ussClassName = "appui-stackview-item";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="item"> The content to add to the StackViewItem. </param>
        public StackViewItem(VisualElement item)
        {
            usageHints = UsageHints.DynamicTransform;
            AddToClassList(ussClassName);
            Add(item);
        }

        /// <summary>
        /// This event is emitted when the item is activated in the stack.
        /// </summary>
        public event Action activated;

        /// <summary>
        /// This event is emitted when the item is deactivated in the stack.
        /// </summary>
        public event Action deactivated;

        /// <summary>
        /// This event is emitted when the item is in the process of being activated in the stack.
        /// </summary>
        public event Action activating;

        /// <summary>
        /// This event is emitted when the item is in the process of being deactivated in the stack.
        /// </summary>
        public event Action deactivating;

        /// <summary>
        /// This event is emitted when the item has been removed from the stack.
        /// It can be used to safely destroy extra data for example.
        /// </summary>
        public event Action removed;

        /// <summary>
        /// The index of the item in the stack.
        /// </summary>
        public int index => parent?.IndexOf(this) ?? -1;

        /// <summary>
        /// The status of the item in the stack.
        /// </summary>
        public StackViewItemStatus status { get; private set; }

        /// <summary>
        /// The StackView that contains this item.
        /// </summary>
        public StackView view { get; internal set; }

        internal void InvokeRemoved()
        {
            status = StackViewItemStatus.Inactive;
            removed?.Invoke();
        }

        internal void InvokeActivated()
        {
            status = StackViewItemStatus.Active;
            activated?.Invoke();
        }

        internal void InvokeActivating()
        {
            status = StackViewItemStatus.Activating;
            activating?.Invoke();
        }

        internal void InvokeDeactivated()
        {
            status = StackViewItemStatus.Inactive;
            deactivated?.Invoke();
        }

        internal void InvokeDeactivating()
        {
            status = StackViewItemStatus.Deactivating;
            deactivating?.Invoke();
        }
    }

    /// <summary>
    /// An animation description. It contains the duration of the animation, the easing function and the callback.
    /// </summary>
    public struct AnimationDescription
    {
        /// <summary>
        /// The duration of the animation in milliseconds.
        /// </summary>
        public int durationMs;

        /// <summary>
        /// The easing function to use for the animation.
        /// </summary>
        public Func<float, float> easing;

        /// <summary>
        /// The callback to call when the animation is running.
        /// </summary>
        public Action<VisualElement, float> callback;
    }

    /// <summary>
    /// A StackView is a container that can contain multiple items. It is similar to a stack of cards.
    /// The items are added to the stack using the Push method. The top item is the current item.
    /// The current item can be removed using the Pop method. The item below the current item becomes the new current item.
    /// The current item can be replaced using the Replace method. The item below the current item is removed and the new item is added.
    /// </summary>
    public class StackView : VisualElement
    {
        readonly Stack<StackViewItem> m_Stack;

        /// <summary>
        /// The main styling class of the StackView. This is the class that is used in the USS file.
        /// </summary>
        public static readonly string ussClassName = "appui-stackview";

        bool m_Initialized;

        ValueAnimation<float> m_CurrentExitAnimation;

        ValueAnimation<float> m_CurrentEnterAnimation;

        /// <summary>
        /// Check if the StackView has any active animation.
        /// </summary>
        public bool isBusy =>
            (m_CurrentExitAnimation?.isRunning ?? false) || (m_CurrentEnterAnimation?.isRunning ?? false);

        /// <summary>
        /// The current item in the stack.
        /// </summary>
        public StackViewItem currentItem => isEmpty ? null : m_Stack.Peek();

        /// <summary>
        /// The depth of the stack.
        /// </summary>
        public int depth => m_Stack.Count;

        /// <summary>
        /// Check if the stack is empty.
        /// </summary>
        public bool isEmpty => depth == 0;

        /// <summary>
        /// The initial item to add to the stack.
        /// </summary>
        public VisualElement initialItem { get; set; }

        /// <summary>
        /// The animation to use on the newly active item when an item is popped from the stack.
        /// </summary>
        public AnimationDescription popEnterAnimation { get; set; }

        /// <summary>
        /// The animation to use on the current item when it is popped from the stack.
        /// </summary>
        public AnimationDescription popExitAnimation { get; set; }

        /// <summary>
        /// The animation to use on the newly active item when it is pushed to the stack.
        /// </summary>
        public AnimationDescription pushEnterAnimation { get; set; }

        /// <summary>
        /// The animation to use on the current item when a new item is pushed to the stack.
        /// </summary>
        public AnimationDescription pushExitAnimation { get; set; }

        /// <summary>
        /// The animation to use on the newly active item when it is replaced in the stack.
        /// </summary>
        public AnimationDescription replaceEnterAnimation { get; set; }

        /// <summary>
        /// The animation to use on the current item when it is replaced in the stack.
        /// </summary>
        public AnimationDescription replaceExitAnimation { get; set; }

        /// <summary>
        /// The constructor of the StackView.
        /// </summary>
        public StackView()
        {
            AddToClassList(ussClassName);

            usageHints = UsageHints.GroupTransform;

            m_Stack = new Stack<StackViewItem>();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (panel != null && paddingRect.IsValid() && !m_Initialized)
            {
                OnCompleted();
                m_Initialized = true;
            }
        }

        void OnCompleted()
        {
            if (initialItem != null)
                Push(initialItem);
        }

        /// <summary>
        /// Removes all items from the stack.
        /// </summary>
        public void ClearStack()
        {
            for (var i = m_Stack.Count - 1; i >= 0; i--)
            {
                RemoveItem(m_Stack.Pop());
            }
        }

        /// <summary>
        /// Pushes an item onto the stack using an optional operation.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="operation">The type of transition to use during the process.</param>
        /// <returns>Returns the item that became current.</returns>
        public StackViewItem Push(VisualElement item, StackViewOperation operation = StackViewOperation.PushTransition)
        {
            var newItem = (item as StackViewItem) ?? new StackViewItem(item);
            newItem.view = this;
            var previousItem = currentItem;

            Exit(previousItem, operation, false);
            m_Stack.Push(newItem);
            Enter(newItem, operation, true);

            return newItem;
        }

        /// <summary>
        /// Pops one or more items off the stack.
        /// </summary>
        /// <param name="item">If the item argument is specified, all items down to (but not including) item
        /// will be popped. If item is null, all items down to (but not including) the first item is popped.</param>
        /// <param name="operation">The type of transition to use during the process.</param>
        /// <returns>Returns the last item removed from the stack.</returns>
        public StackViewItem Pop(StackViewItem item, StackViewOperation operation = StackViewOperation.PopTransition)
        {
            StackViewItem lastRemoved = null;

            if (depth > 1)
            {
                while (m_Stack.TryPeek(out var peekedItem))
                {
                    if (depth == 1 || item == peekedItem)
                        break;

                    lastRemoved = m_Stack.Pop();
                    Exit(lastRemoved, operation, true);
                }
                Enter(currentItem, operation, false);
            }

            return lastRemoved;
        }

        /// <summary>
        /// Pops one or more items off the stack. Only the current item is popped.
        /// </summary>
        /// <param name="operation">The type of transition to use during the process.</param>
        /// <returns>Returns the last item removed from the stack.</returns>
        public StackViewItem Pop(StackViewOperation operation = StackViewOperation.PopTransition)
        {
            StackViewItem lastRemoved = null;

            if (depth > 1)
            {
                lastRemoved = m_Stack.Pop();
                Exit(lastRemoved, operation, true);
                Enter(currentItem, operation, false);
            }

            return lastRemoved;
        }

        /// <summary>
        /// Replaces one or more items on the stack with the specified item and optional operation.
        /// </summary>
        /// <param name="target">If the target argument is specified, all items down to the target item will be replaced.
        /// If target is null, all items in the stack will be replaced.
        /// If not specified, only the top item will be replaced.
        /// If the target argument is specified, all items down to the target item will be replaced.
        /// If target is null, all items in the stack will be replaced.
        /// If not specified, only the top item will be replaced.</param>
        /// <param name="item">The item that will be used as replacement.</param>
        /// <param name="operation">The type of transition to use during the process.</param>
        /// <returns>Returns the item that became current.</returns>
        public StackViewItem Replace(StackViewItem target, VisualElement item, StackViewOperation operation = StackViewOperation.ReplaceTransition)
        {
            return null;
        }

        void Enter(StackViewItem enteredItem, StackViewOperation operation, bool add)
        {
            if (enteredItem == null)
                return;

            var anim = operation switch
            {
                StackViewOperation.PopTransition => popEnterAnimation,
                StackViewOperation.PushTransition => pushEnterAnimation,
                StackViewOperation.ReplaceTransition => replaceEnterAnimation,
                _ => default(AnimationDescription),
            };

            if (add)
                Add(enteredItem);
            enteredItem.InvokeActivating();
            m_CurrentExitAnimation = enteredItem.experimental.animation
                .Start(0, 1f, anim.durationMs, anim.callback)
                .Ease(anim.easing)
                .OnCompleted(enteredItem.InvokeActivated);
        }

        void Exit(StackViewItem poppedItem, StackViewOperation operation, bool remove)
        {
            if (poppedItem == null)
                return;

            var anim = operation switch
            {
                StackViewOperation.PopTransition => popExitAnimation,
                StackViewOperation.PushTransition => pushExitAnimation,
                StackViewOperation.ReplaceTransition => replaceExitAnimation,
                _ => default(AnimationDescription),
            };

            poppedItem.InvokeDeactivating();
            m_CurrentExitAnimation = poppedItem.experimental.animation
                .Start(1f, 0, anim.durationMs, anim.callback)
                .Ease(anim.easing)
                .OnCompleted(() =>
                {
                    poppedItem.InvokeDeactivated();
                    if (remove)
                        RemoveItem(poppedItem);
                });
        }

        void RemoveItem(StackViewItem item)
        {
            Remove(item);
            item.InvokeRemoved();
            item.view = null;
        }

        /// <summary>
        /// Defines the UxmlFactory for the StackView.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<StackView, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="StackView"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {

        }
    }
}
