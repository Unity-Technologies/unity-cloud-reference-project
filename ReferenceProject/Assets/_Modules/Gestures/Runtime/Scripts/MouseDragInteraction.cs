using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.ReferenceProject.Gestures
{
    /// <summary>
    ///     Mouse drag interaction.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class MouseDragInteraction : IInputInteraction
    {
        static MouseDragInteraction()
        {
            InputSystem.RegisterInteraction<MouseDragInteraction>();
        }

        public void Reset()
        {
            //Nothing to reset
        }

        public void Process(ref InputInteractionContext context)
        {
            if (context.timerHasExpired)
            {
                context.Performed();
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
                        if (context.action.controls[0] is ButtonControl { isPressed: false })
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
    }
}
