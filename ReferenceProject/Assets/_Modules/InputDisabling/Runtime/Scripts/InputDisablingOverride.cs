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
                m_ToolUIController.ToolOpened.AddListener(() => RegisterSelf(OverrideTypes.Opened));
                m_ToolUIController.ToolClosed.AddListener(() => UnregisterSelf(OverrideTypes.Opened));
            }

            if ((m_OverrideTypes & OverrideTypes.PointerEntered) != 0)
            {
                m_ToolUIController.ToolPointerEntered.AddListener(() => RegisterSelf(OverrideTypes.PointerEntered));
                m_ToolUIController.ToolPointerExited.AddListener(() => UnregisterSelf(OverrideTypes.PointerEntered));
            }

            if ((m_OverrideTypes & OverrideTypes.Focused) != 0)
            {
                m_ToolUIController.ToolFocusIn.AddListener(() => RegisterSelf(OverrideTypes.Focused));
                m_ToolUIController.ToolFocusOut.AddListener(() => UnregisterSelf(OverrideTypes.Focused));
            }
        }

        void OnDestroy()
        {
            m_ToolUIController.ToolOpened.RemoveListener(() => RegisterSelf(OverrideTypes.Opened));
            m_ToolUIController.ToolClosed.RemoveListener(() => UnregisterSelf(OverrideTypes.Opened));

            m_ToolUIController.ToolPointerEntered.RemoveListener(() => RegisterSelf(OverrideTypes.PointerEntered));
            m_ToolUIController.ToolPointerExited.RemoveListener(() => UnregisterSelf(OverrideTypes.PointerEntered));

            m_ToolUIController.ToolFocusIn.RemoveListener(() => RegisterSelf(OverrideTypes.Focused));
            m_ToolUIController.ToolFocusOut.RemoveListener(() => UnregisterSelf(OverrideTypes.Focused));
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
            Focused = 4
        }
    }
}
