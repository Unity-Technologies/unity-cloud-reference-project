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
    public class TwoFingerDistanceComposite : InputBindingComposite<TouchState>
    {
        [Preserve]
        static TwoFingerDistanceComposite()
        {
            InputSystem.RegisterBindingComposite<TwoFingerDistanceComposite>("TwoFinger/Distance");
        }

        public override TouchState ReadValue(ref InputBindingCompositeContext context)
        {
            var touch1 = context.ReadValue<TouchState, TouchStateComparer>(Finger1);
            var touch2 = context.ReadValue<TouchState, TouchStateComparer>(Finger2);

            if (!touch1.isInProgress)
                return touch1;

            if (!touch2.isInProgress)
                return touch2;

            var distance = touch1;
            distance.position = touch1.position - touch2.position;

            return distance;
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var distance = ReadValue(ref context);
            return distance.isInProgress ? distance.position.magnitude : 0.0f;
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

        #region Fields

        [InputControl(layout = "Finger")]
        [UsedImplicitly]
        public int Finger1;

        [InputControl(layout = "Finger")]
        [UsedImplicitly]
        public int Finger2;

        #endregion
    }
}
