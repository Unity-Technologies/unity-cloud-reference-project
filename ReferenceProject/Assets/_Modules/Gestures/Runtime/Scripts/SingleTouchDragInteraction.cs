using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.ReferenceProject.Gestures
{
    /// <summary>
    ///     Single touch drag interaction.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class SingleTouchDragInteraction : IInputInteraction
    {
        static SingleTouchDragInteraction()
        {
            InputSystem.RegisterInteraction<SingleTouchDragInteraction>();
        }

        public void Reset()
        {
            // Nothing to reset
        }

        public void Process(ref InputInteractionContext context)
        {
            if (context.timerHasExpired)
            {
                context.Performed();
                return;
            }
            
            int activeTouches = CountActiveTouches();

            if (activeTouches > 1)
            {
                context.Canceled();
                return;
            }

            switch (context.phase)
            {
                case InputActionPhase.Disabled:
                    break;

                case InputActionPhase.Waiting:
                    if (context.ControlIsActuated())
                    {
                        context.Started();
                        context.SetTimeout(float.PositiveInfinity);
                    }
                    break;

                case InputActionPhase.Started:
                    context.PerformedAndStayPerformed();
                    break;

                case InputActionPhase.Performed:
                    if (context.ControlIsActuated())
                    {
                        context.PerformedAndStayPerformed();
                    }
                    else
                    {
                        var pointer = context.action.controls[0] as Pointer;
                        if (context.action.controls[0] is TouchControl { isInProgress: false })
                        {
                            context.Canceled();
                        }

                        if (pointer != null && !pointer.IsPressed())
                        {
                            context.Canceled();
                        }
                    }
                    break;

                case InputActionPhase.Canceled:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, $"Context.phase field is invalid, phase = {context.phase}");
            }
        }
        
        static int CountActiveTouches()
        {
            var activeTouches = Touchscreen.current.touches
                .Select(touch => touch.phase.ReadValue())
                .Where(phase => phase is TouchPhase.Began or TouchPhase.Moved);
            return activeTouches.Count();
        }
    }
}
