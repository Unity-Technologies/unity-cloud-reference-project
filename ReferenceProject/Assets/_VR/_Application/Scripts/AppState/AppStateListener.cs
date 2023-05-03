using System;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;

namespace Unity.ReferenceProject.VR
{
    [RequireComponent(typeof(AppState))]
    public abstract class AppStateListener : MonoBehaviour
    {
        AppState m_AppState;

        protected virtual void Awake()
        {
            m_AppState = GetComponent<AppState>();
            m_AppState.StateEntered += StateEntered;
            m_AppState.StateExited += StateExited;
        }

        protected virtual void OnDestroy()
        {
            m_AppState.StateEntered -= StateEntered;
            m_AppState.StateExited -= StateExited;
        }

        protected abstract void StateEntered();

        protected abstract void StateExited();
    }
}
