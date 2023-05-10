using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.ReferenceProject.Gestures
{
    /// <summary>
    ///     Two Finger Drag Gesture interaction.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public class TwoFingerDragGestureInteraction : GestureInteraction<TwoFingerDragGesture>
    {
        [Preserve]
        static TwoFingerDragGestureInteraction()
        {
            InputSystem.RegisterInteraction<TwoFingerDragGestureInteraction>();
        }

        protected override GestureRecognizer<TwoFingerDragGesture> CreateRecognizer(TouchControl touch1, TouchControl touch2)
        {
            return new TwoFingerDragGestureRecognizer(touch1, touch2);
        }
    }
}
