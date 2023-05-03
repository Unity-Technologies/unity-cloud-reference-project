using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A Manipulator that simulates a click when the target element is "submitted".
    /// </summary>
    public class Submittable : Clickable
    {
        /// <summary>
        /// Construct a Submittable.
        /// </summary>
        /// <param name="handler"> A callback invoked when the target element is submitted.</param>
        /// <param name="delay"> The delay before the callback is invoked.</param>
        /// <param name="interval"> The interval between invocations of the callback.</param>
        public Submittable(Action handler, long delay, long interval)
            : base(handler, delay, interval) { }

        /// <summary>
        /// Construct a Submittable.
        /// </summary>
        /// <param name="handler"> A callback invoked when the target element is submitted.</param>
        public Submittable(Action<EventBase> handler)
            : base(handler) { }

        /// <summary>
        /// Construct a Submittable.
        /// </summary>
        /// <param name="handler"> A callback invoked when the target element is submitted.</param>
        public Submittable(Action handler)
            : base(handler) { }

        /// <summary>
        /// Called to register event callbacks from the target element.
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            base.RegisterCallbacksOnTarget();

            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        /// <summary>
        /// Called to unregister event callbacks from the target element.
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            base.UnregisterCallbacksFromTarget();

            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.UnregisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        void OnSubmit(NavigationSubmitEvent evt)
        {
            SimulateSingleClickInternal(evt);
            evt.StopPropagation();
            evt.PreventDefault();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
#if !UNITY_2022_2_OR_NEWER
            if (evt.keyCode.IsSubmitType())
            {
                SimulateSingleClickInternal(evt);
                evt.StopPropagation();
                evt.PreventDefault();
            }
#endif
        }
    }
}
