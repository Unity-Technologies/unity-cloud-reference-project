using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Unity.ReferenceProject.InputDisabling
{
    public interface IInputDisablingSubscriber : IInputDisablingBaseObject
    {
        public void Enable();

        public void Disable();
    }

    public class InputDisablingSubscriber : MonoBehaviour, IInputDisablingSubscriber
    {
        [SerializeField]
        bool m_IsDisableGameObject;

        [SerializeField]
        UnityEvent m_OnInputsEnable;

        [SerializeField]
        UnityEvent m_OnInputsDisable;

        IInputDisablingManager m_InputDisablingManager;

        [Inject]
        void Setup(IInputDisablingManager inputDisablingManager)
        {
            m_InputDisablingManager = inputDisablingManager;
        }

        void Awake()
        {
            RegisterSelf();
        }

        void OnDestroy()
        {
            UnregisterSelf();
        }

        public GameObject GameObject => gameObject;

        public void Enable()
        {
            if (m_IsDisableGameObject)
                gameObject.SetActive(true);

            m_OnInputsEnable?.Invoke();
        }

        public void Disable()
        {
            if (m_IsDisableGameObject)
                gameObject.SetActive(false);

            m_OnInputsDisable?.Invoke();
        }

        void RegisterSelf()
        {
            m_InputDisablingManager.AddSubscriber(this);
        }

        void UnregisterSelf()
        {
            m_InputDisablingManager.RemoveSubscriber(this);
        }
    }
}
