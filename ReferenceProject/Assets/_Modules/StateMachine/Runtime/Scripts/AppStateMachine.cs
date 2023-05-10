using System;
using UnityEngine;

namespace Unity.ReferenceProject.StateMachine
{
    public class AppStateMachine : MonoBehaviour
    {
        [SerializeField]
        AppState m_InitialState;

        readonly AppStateController m_AppStateController = new();
        
        public IAppStateController AppStateController => m_AppStateController;

        void Start()
        {
            m_AppStateController.PrepareTransition(m_InitialState).Apply();
        }
    }
}
