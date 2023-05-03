using System;

namespace Unity.ReferenceProject.StateMachine
{
    public interface IAppStateController
    {
        public event Action<AppState> StateExit;
        public event Action<AppState> StateEnter;

        public IAppStateTransition PrepareTransition(AppState nextState);
    }

    public interface IAppStateTransition
    {
        public IAppStateTransition OnAfterExit(Action action);

        public IAppStateTransition OnBeforeEnter(Action action);

        void Apply();
    }

    public class AppStateController : IAppStateController
    {
        AppState m_CurrentState;
        public event Action<AppState> StateExit;
        public event Action<AppState> StateEnter;

        public IAppStateTransition PrepareTransition(AppState nextState)
        {
            return new AppStateTransition(this, nextState);
        }

        void ApplyTransition(AppStateTransition transition)
        {
            if (m_CurrentState != null)
            {
                m_CurrentState.ExitState();
                StateExit?.Invoke(m_CurrentState);
                transition.InvokeAfterExit();
            }

            m_CurrentState = transition.NextState;

            if (m_CurrentState != null)
            {
                transition.InvokeBeforeEnter();
                m_CurrentState.EnterState();
                StateEnter?.Invoke(m_CurrentState);
            }
        }

        class AppStateTransition : IAppStateTransition
        {
            readonly AppStateController m_Controller;
            internal readonly AppState NextState;

            internal AppStateTransition(AppStateController controller, AppState nextState)
            {
                m_Controller = controller;
                NextState = nextState;
            }

            public IAppStateTransition OnAfterExit(Action action)
            {
                m_AfterExit += action;
                return this;
            }

            public IAppStateTransition OnBeforeEnter(Action action)
            {
                m_BeforeEnter += action;
                return this;
            }

            public void Apply()
            {
                m_Controller.ApplyTransition(this);
            }

            event Action m_AfterExit;
            event Action m_BeforeEnter;

            internal void InvokeAfterExit()
            {
                m_AfterExit?.Invoke();
            }

            internal void InvokeBeforeEnter()
            {
                m_BeforeEnter?.Invoke();
            }
        }
    }
}
