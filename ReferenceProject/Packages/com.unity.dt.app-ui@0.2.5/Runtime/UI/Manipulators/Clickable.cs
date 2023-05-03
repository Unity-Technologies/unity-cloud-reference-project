using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    [Flags]
    enum PseudoStates
    {
        Active = 1,
        Hover = 2,
        Checked = 8,
        Disabled = 32, // 0x00000020
        Focus = 64, // 0x00000040
        Root = 128, // 0x00000080
    }

    static class ScrollWaitDefinitions
    {
        public const int firstWait = 250; // ms
        public const int regularWait = 30; // ms
    }

    /// <summary>
    /// Clickable Manipulator, used on <see cref="Button"/> elements.
    /// </summary>
    public class Clickable : UIElements.Clickable
    {
        internal static readonly PropertyInfo pseudoStateProperty =
            typeof(VisualElement).GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler">The handler to invoke when the click event is triggered.</param>
        /// <param name="delay"> The delay before the first click event is triggered.</param>
        /// <param name="interval"> The interval between two click events.</param>
        public Clickable(Action handler, long delay, long interval)
            : base(handler, delay, interval)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler"> The handler to invoke when the click event is triggered.</param>
        public Clickable(Action<EventBase> handler)
            : base(handler)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler"> The handler to invoke when the click event is triggered.</param>
        public Clickable(Action handler)
            : base(handler)
        {
        }

        /// <summary>
        /// This method processes the move event sent to the target Element.
        /// </summary>
        /// <param name="evt"> The base event to process.</param>
        /// <param name="localPosition"> The local position of the pointer.</param>
        protected override void ProcessMoveEvent(EventBase evt, Vector2 localPosition)
        {
            //evt.StopPropagation();
        }

        /// <summary>
        /// Invoke the click event.
        /// </summary>
        /// <param name="evt">The base event to use to invoke the click.</param>
        internal void InvokeClick(EventBase evt) => Invoke(evt);

        /// <summary>
        /// Simulate a single click on the target element.
        /// </summary>
        /// <param name="evt">The base event to use to invoke the click.</param>
        internal void SimulateSingleClickInternal(EventBase evt)
        {
            if (pseudoStateProperty != null && target != null)
            {
                var pseudoStates = (PseudoStates)(int)pseudoStateProperty.GetValue(target);
                pseudoStateProperty.SetValue(target, (int)(pseudoStates | PseudoStates.Active));
                target.schedule
                    .Execute(() =>
                    pseudoStateProperty.SetValue(target,
                        (int)((PseudoStates)(int)pseudoStateProperty.GetValue(target) & ~PseudoStates.Active)))
                    .ExecuteLater(16L);
            }
            InvokeClick(evt);
        }

        /// <summary>
        /// Force the active pseudo state on the target element.
        /// </summary>
        public void ForceActivePseudoState()
        {
            if (pseudoStateProperty != null && target != null)
            {
                var pseudoStates = (PseudoStates)(int)pseudoStateProperty.GetValue(target);
                pseudoStateProperty.SetValue(target, (int)(pseudoStates | PseudoStates.Active));
            }
        }
    }
}
