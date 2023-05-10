using System;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.StateMachine
{
    [Serializable]
    public abstract class AppState : MonoBehaviour
    {
        protected IAppStateController AppStateController;

        [Inject]
        void Setup(IAppStateController appStateController)
        {
            AppStateController = appStateController;
        }

        public bool IsActive { get; private set; }
        public event Action StateEntered;
        public event Action StateExited;

        public void EnterState()
        {
            IsActive = true;
            StateEntered?.Invoke();
            EnterStateInternal();
        }

        public void ExitState()
        {
            IsActive = false;
            StateExited?.Invoke();
            ExitStateInternal();
        }

        protected virtual void EnterStateInternal() { }

        protected virtual void ExitStateInternal() { }
    }
}
