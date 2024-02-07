using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class PresenterUI : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;
        
        PresentationManager m_PresentationManager;
        VisualElement m_PresenterBorder;
        
        [Inject]
        public void Setup(PresentationManager presentationManager)
        {
            m_PresentationManager = presentationManager;
        }

        void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                InitUIToolkit(uiDocument.rootVisualElement);
            }

            m_PresentationManager.PresentationStarted += OnPresentationStarted;
            m_PresentationManager.PresentationStopped += OnPresentationStopped;
        }

        void OnDestroy()
        {
            if(m_PresentationManager)
            {
                m_PresentationManager.PresentationStarted -= OnPresentationStarted;
                m_PresentationManager.PresentationStopped -= OnPresentationStopped;
            }
        }

        void OnPresentationStopped(ParticipantId obj)
        {
            Utils.SetVisible(m_PresenterBorder, false);
        }

        void OnPresentationStarted(ParticipantId obj)
        {
            Utils.SetVisible(m_PresenterBorder, true);
        }

        void InitUIToolkit(VisualElement rootVisualElement)
        {
            m_PresenterBorder = rootVisualElement.Q("Border");
            Utils.SetVisible(m_PresenterBorder, false);
            m_UIDocument.rootVisualElement.AddToClassList("container__presenter-height");
            m_UIDocument.rootVisualElement.Add(m_PresenterBorder);
        }
    }
}
