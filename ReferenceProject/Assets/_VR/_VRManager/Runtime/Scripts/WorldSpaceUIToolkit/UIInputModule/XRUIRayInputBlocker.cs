using System;
using Unity.ReferenceProject.UIInputBlocker;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;

namespace Unity.ReferenceProject.VRManager
{
    [RequireComponent(typeof(XRRayInteractor))]
    public class XRUIRayInputBlocker : MonoBehaviour
    {
        IUIInputBlockerEventsDispatcher m_Dispatcher;

        XRUIInputModuleUIToolkit m_InputModule;

        int m_PointerID;
        XRRayInteractor m_RayInteractor;

        Transform m_RayOrigin;

        [Inject]
        public void Setup(IUIInputBlockerEventsDispatcher dispatcher)
        {
            m_Dispatcher = dispatcher;
        }

        void Start()
        {
            m_RayInteractor = GetComponent<XRRayInteractor>();
            m_RayOrigin = m_RayInteractor.rayOriginTransform;
            if (m_RayInteractor.TryGetUIModel(out var model))
            {
                m_PointerID = model.pointerId;
            }
            else
            {
                Debug.LogWarning($"Can't get TrackedDeviceModel. {nameof(XRUIRayInputBlocker)} has been disabled!");
                enabled = false;
            }
        }

        void OnEnable()
        {
            m_InputModule = EventSystem.current?.GetComponent<XRUIInputModuleUIToolkit>();
            if (m_InputModule)
            {
                m_InputModule.PointerDown += OnPointerDown;
            }
            else
            {
                Debug.LogWarning($"Can't find {nameof(XRUIInputModuleUIToolkit)}. {nameof(XRUIRayInputBlocker)} has been disabled!");
                enabled = false;
            }
        }

        void OnDisable()
        {
            if (m_InputModule)
            {
                m_InputModule.PointerDown -= OnPointerDown;
            }
        }

        void OnPointerDown(GameObject target, PointerEventData pointerEventData)
        {
            if (!target && pointerEventData.pointerId == m_PointerID && m_RayOrigin)
            {
                if (m_Dispatcher != null)
                {
                    m_Dispatcher.DispatchRay(new Ray(m_RayOrigin.position, m_RayOrigin.forward));
                }
                else
                {
                    Debug.LogWarning($"Missing reference to {nameof(IUIInputBlockerEventsDispatcher)}. Input data can't be dispatched!");
                }
            }
        }
    }
}
