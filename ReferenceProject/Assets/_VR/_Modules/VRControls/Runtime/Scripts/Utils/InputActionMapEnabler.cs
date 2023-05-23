using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.VR.VRControls
{
    public class InputActionMapEnabler : MonoBehaviour
    {
        [SerializeField]
        InputActionAsset m_InputAction;

        [SerializeField]
        List<string> m_InputActionMaps;

        void OnEnable()
        {
            foreach (var actionMap in m_InputAction.actionMaps.Where(actionMap => m_InputActionMaps.Contains(actionMap.name)))
            {
                actionMap.Enable();
            }
        }

        void OnDisable()
        {
            foreach (var actionMap in m_InputAction.actionMaps.Where(actionMap => m_InputActionMaps.Contains(actionMap.name)))
            {
                actionMap.Disable();
            }
        }
    }
}
