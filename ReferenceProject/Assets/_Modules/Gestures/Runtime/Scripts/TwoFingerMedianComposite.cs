using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.ReferenceProject.Gestures
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public class TwoFingerMedianComposite : InputBindingComposite<TouchState>
    {
        [InputControl(layout = "Finger")]
        [UsedImplicitly]
        int m_Finger1;

        [InputControl(layout = "Finger")]
        [UsedImplicitly]
        int m_Finger2;
        
        [Preserve]
        static TwoFingerMedianComposite()
        {
            InputSystem.RegisterBindingComposite<TwoFingerMedianComposite>("TwoFinger/Median");
        }

        public override TouchState ReadValue(ref InputBindingCompositeContext context)
        {
            var touch1 = context.ReadValue<TouchState, TouchStateComparer>(m_Finger1);
            var touch2 = context.ReadValue<TouchState, TouchStateComparer>(m_Finger2);

            if (!touch1.isInProgress)
                return touch1;

            if (!touch2.isInProgress)
                return touch2;

            var median = touch1;
            median.position = (touch1.position + touch2.position) / 2.0f;

            return median;
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var median = ReadValue(ref context);
            return median.isInProgress ? median.position.magnitude : 0.0f;
        }

        struct TouchStateComparer : IComparer<TouchState>
        {
            public int Compare(TouchState touch1, TouchState touch2)
            {
                if (touch1.Equals(touch2))
                    return 0;

                if (touch1.touchId > touch2.touchId)
                    return 1;

                return -1;
            }
        }
    }
}
