using System;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;

namespace Unity.ReferenceProject
{
    [Serializable]
    public sealed class InitialState : AppState
    {
        [SerializeField]
        AppState m_NextState;

        protected override void EnterStateInternal()
        {
            AppStateController.PrepareTransition(m_NextState).Apply();
        }
    }
}
