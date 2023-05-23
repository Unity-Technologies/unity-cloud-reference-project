using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.InputDisabling
{
    public class SwipeInputDisabling : IInputDisablingOverride
    {
        public GameObject GameObject => m_GameObject;

        bool m_IsInputBlocked;
        readonly IInputDisablingManager m_InputDisablingManager;
        readonly GameObject m_GameObject;

        public SwipeInputDisabling(GameObject gameObject, IInputDisablingManager inputDisablingManager, VisualElement visualElement)
        {
            m_GameObject = gameObject;
            m_InputDisablingManager = inputDisablingManager;

            visualElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            visualElement.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut, TrickleDown.TrickleDown);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!m_IsInputBlocked)
            {
                m_IsInputBlocked = true;
                m_InputDisablingManager.AddOverride(this);
            }
        }

        void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (m_IsInputBlocked)
            {
                m_IsInputBlocked = false;
                m_InputDisablingManager.RemoveOverride(this);
            }
        }
    }
}
