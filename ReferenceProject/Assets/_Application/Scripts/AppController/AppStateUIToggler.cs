using System;
using System.Collections.Generic;
using Unity.ReferenceProject.StateMachine;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject
{
    [RequireComponent(typeof(AppState))]
    public class AppStateUIToggler : MonoBehaviour
    {
        [SerializeField]
        List<UIDocument> m_AssociatedUI;

        AppState m_AppState;

        void Awake()
        {
            m_AppState = GetComponent<AppState>();
            m_AppState.StateEntered += ShowUI;
            m_AppState.StateExited += HideUI;

            SetVisibility(m_AppState.IsActive);
        }

        void OnDestroy()
        {
            m_AppState.StateEntered -= ShowUI;
            m_AppState.StateExited -= HideUI;
        }

        void ShowUI()
        {
            SetVisibility(true);
        }

        void HideUI()
        {
            SetVisibility(false);
        }

        void SetVisibility(bool visible)
        {
            foreach (var uiDocument in m_AssociatedUI)
            {
                SetVisibility(uiDocument, visible);
            }
        }

        static void SetVisibility(UIDocument uiDocument, bool visible)
        {
            uiDocument.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
