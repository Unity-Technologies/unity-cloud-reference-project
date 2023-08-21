using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.InputDisabling
{
    public interface IInputDisablingOverride : IInputDisablingBaseObject { }

    [RequireComponent(typeof(ToolUIController))]
    public class InputDisablingOverride : MonoBehaviour, IInputDisablingOverride
    {
        [SerializeField]
        OverrideTypes m_OverrideTypes;

        readonly HashSet<OverrideTypes> m_ActiveOverrideTypes = new();
        IInputDisablingManager m_InputDisablingManager;

        ToolUIController m_ToolUIController;

        [Inject]
        void Setup(IInputDisablingManager inputDisablingManager)
        {
            m_InputDisablingManager = inputDisablingManager;
        }

        void Awake()
        {
            m_ToolUIController = GetComponent<ToolUIController>();

            if ((m_OverrideTypes & OverrideTypes.Opened) != 0)
            {
                m_ToolUIController.ToolOpened += OnToolOpened;
                m_ToolUIController.ToolClosed += OnToolClosed;
            }

            if ((m_OverrideTypes & OverrideTypes.PointerEntered) != 0)
            {
                m_ToolUIController.ToolPointerEntered += OnToolPointerEntered;
                m_ToolUIController.ToolPointerExited += OnToolPointerExited;
            }

            if ((m_OverrideTypes & OverrideTypes.Focused) != 0)
            {
                m_ToolUIController.ToolFocusIn += OnToolFocusIn;
                m_ToolUIController.ToolFocusOut += OnToolFocusOut;
            }

            if ((m_OverrideTypes & OverrideTypes.PointerDown) != 0)
            {
                m_ToolUIController.ToolPointerDown += OnToolPointerDown;
                m_ToolUIController.ToolPointerUp += OnToolPointerUp;
            }

            if ((m_OverrideTypes & OverrideTypes.PointerCapture) != 0)
            {
                m_ToolUIController.ToolPointerCapture += OnToolPointerCapture;
                m_ToolUIController.ToolPointerCaptureOut += OnToolPointerCaptureOut;
            }
        }

        void OnToolOpened()
        {
            RegisterSelf(OverrideTypes.Opened);
        }

        void OnToolClosed()
        {
            UnregisterSelf(OverrideTypes.Opened);
        }

        void OnToolPointerEntered()
        {
            RegisterSelf(OverrideTypes.PointerEntered);
        }

        void OnToolPointerExited()
        {
            UnregisterSelf(OverrideTypes.PointerEntered);
        }

        void OnToolFocusIn()
        {
            RegisterSelf(OverrideTypes.Focused);
        }

        void OnToolFocusOut()
        {
            UnregisterSelf(OverrideTypes.Focused);
        }

        void OnToolPointerDown()
        {
            RegisterSelf(OverrideTypes.PointerDown);
        }

        void OnToolPointerUp()
        {
            UnregisterSelf(OverrideTypes.PointerDown);
        }

        void OnToolPointerCapture()
        {
            RegisterSelf(OverrideTypes.PointerCapture);
        }

        void OnToolPointerCaptureOut()
        {
            UnregisterSelf(OverrideTypes.PointerCapture);
        }

        void OnDestroy()
        {
            m_ToolUIController.ToolOpened -= OnToolOpened;
            m_ToolUIController.ToolClosed -= OnToolClosed;
            m_ToolUIController.ToolPointerEntered -= OnToolPointerEntered;
            m_ToolUIController.ToolPointerExited -= OnToolPointerExited;
            m_ToolUIController.ToolFocusIn -= OnToolFocusIn;
            m_ToolUIController.ToolFocusOut -= OnToolFocusOut;
            m_ToolUIController.ToolPointerDown -= OnToolPointerDown;
            m_ToolUIController.ToolPointerUp -= OnToolPointerUp;
            m_ToolUIController.ToolPointerDown -= OnToolPointerCapture;
            m_ToolUIController.ToolPointerUp -= OnToolPointerCaptureOut;
        }

        public GameObject GameObject => gameObject;

        void RegisterSelf(OverrideTypes overrideTypes)
        {
            if (m_ActiveOverrideTypes.Count == 0)
                m_InputDisablingManager.AddOverride(this);

            m_ActiveOverrideTypes.Add(overrideTypes);
        }

        void UnregisterSelf(OverrideTypes overrideTypes)
        {
            m_ActiveOverrideTypes.Remove(overrideTypes);

            if (m_ActiveOverrideTypes.Count == 0)
                m_InputDisablingManager.RemoveOverride(this);
        }

        [Flags]
        enum OverrideTypes
        {
            None = 0,
            Opened = 1,
            PointerEntered = 2,
            Focused = 4,
            PointerDown = 8,
            PointerCapture = 16
        }
    }
}
