using System;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Stats
{
    public class GlobalStatsUIController : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;
        
        [SerializeField]
        StyleSheet m_AdditionalStyle;

        [SerializeField]
        string m_RootElementName = "stats-entries";

        IGlobalStats m_GlobalStats;
        VisualElement m_GlobalStatsRoot;

        VisualElement m_RootVisualElement;

        bool m_IsDisplayed;
        
        public bool IsDisplayed
        {
            get => m_IsDisplayed;
            
            set
            {
                m_IsDisplayed = value;
                
                if (m_RootVisualElement != null)
                {
                    SetStatsDisplayed(value);
                }
            }
        }

        [Inject]
        void Setup(IGlobalStats stats)
        {
            m_GlobalStats = stats;
        }

        void Awake()
        {
            if (m_UIDocument != null)
            {
                CreateVisualTree(m_UIDocument);   
            }
        }

        public void CreateVisualTree(UIDocument document)
        {
            m_RootVisualElement = document.rootVisualElement.Q(m_RootElementName);
            if (m_AdditionalStyle != null)
            {
                m_RootVisualElement.styleSheets.Add(m_AdditionalStyle);
            }
            
            if (m_GlobalStatsRoot == null)
            {
                m_GlobalStatsRoot = m_GlobalStats.CreateVisualTree();

                m_RootVisualElement.Add(m_GlobalStatsRoot);
            }

            SetStatsDisplayed(IsDisplayed);
        }

        void SetStatsDisplayed(bool isDisplayed)
        {
            m_GlobalStats.SetDisplayed(isDisplayed);
            Common.Utils.SetVisible(m_RootVisualElement, isDisplayed);
        }
    }
}
