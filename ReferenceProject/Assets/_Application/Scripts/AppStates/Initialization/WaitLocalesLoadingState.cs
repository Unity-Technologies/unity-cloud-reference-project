using System;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    [Serializable]
    public sealed class WaitLocalesLoadingState : AppState
    {
        [SerializeField]
        AppState m_NextState;

        IAppLocalization m_AppLocalization;

        [Inject]
        public void Setup(IAppLocalization localization)
        {
            m_AppLocalization = localization;
        }

        protected override void EnterStateInternal()
        {
            m_AppLocalization.LocalizationLoaded += OnLocalizationLoaded;
        }

        protected override void ExitStateInternal()
        {
            m_AppLocalization.LocalizationLoaded -= OnLocalizationLoaded;
        }

        void OnLocalizationLoaded()
        {
            AppStateController.PrepareTransition(m_NextState).Apply();
        }
    }
}
