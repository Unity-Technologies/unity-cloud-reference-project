using System;
using Unity.ReferenceProject.WorldSpaceUIToolkit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.ReferenceProject.VR
{
    public class InputSingleUnifiedPointer : MonoBehaviour
    {
        XRUIInputModuleUIToolkit m_InputModule;

        void Awake()
        {
            m_InputModule = EventSystem.current?.GetComponent<XRUIInputModuleUIToolkit>();
        }

        public void Enable()
        {
            m_InputModule.SingleUnifiedPointer = true;
        }

        public void Disable()
        {
            m_InputModule.SingleUnifiedPointer = false;
        }
    }
}
