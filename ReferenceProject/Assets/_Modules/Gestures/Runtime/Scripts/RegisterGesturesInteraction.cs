using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.Gestures
{
    public class RegisterGesturesInteraction : MonoBehaviour
    {
        const string m_TwoFingerMedian = "TwoFinger/Median";
        const string m_TwoFingerDistance = "TwoFinger/Distance";
        [SerializeField]
        bool m_RegisterMouseDragInteraction = true;
        [SerializeField]
        bool m_RegisterTwoFingerDragInteraction = true;
        [SerializeField]
        bool m_RegisterPinchInteraction = true;

        public void Awake()
        {
            if (m_RegisterMouseDragInteraction)
            {
                InputSystem.RegisterBindingComposite<MouseDragComposite>();
                InputSystem.RegisterInteraction<MouseDragInteraction>();
            }

            if (m_RegisterTwoFingerDragInteraction)
            {
                InputSystem.RegisterBindingComposite<TwoFingerMedianComposite>(m_TwoFingerMedian);
                InputSystem.RegisterInteraction<TwoFingerDragGestureInteraction>();
            }

            if (m_RegisterPinchInteraction)
            {
                InputSystem.RegisterBindingComposite<TwoFingerDistanceComposite>(m_TwoFingerDistance);
                InputSystem.RegisterInteraction<PinchGestureInteraction>();
            }
        }
    }
}
