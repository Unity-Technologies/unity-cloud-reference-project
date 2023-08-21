using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.Gestures
{
    public class RegisterGesturesInteraction : MonoBehaviour
    {
        static readonly string k_TwoFingerMedian = "TwoFinger/Median";
        static readonly string k_TwoFingerDistance = "TwoFinger/Distance";
        
        [SerializeField]
        bool m_RegisterMouseDragInteraction = true;
        
        [SerializeField]
        bool m_RegisterSingleTouchDragInteraction = true;
        
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
                InputSystem.RegisterBindingComposite<TwoFingerMedianComposite>(k_TwoFingerMedian);
                InputSystem.RegisterInteraction<TwoFingerDragGestureInteraction>();
            }

            if (m_RegisterPinchInteraction)
            {
                InputSystem.RegisterBindingComposite<TwoFingerDistanceComposite>(k_TwoFingerDistance);
                InputSystem.RegisterInteraction<PinchGestureInteraction>();
            }

            if (m_RegisterSingleTouchDragInteraction)
            {
                InputSystem.RegisterInteraction<SingleTouchDragInteraction>();
            }
        }
    }
}
