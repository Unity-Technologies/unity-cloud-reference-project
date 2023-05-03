using System;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Settings
{
    public class GlobalSettingsUIController : ToolUIController
    {
        [SerializeField]
        StyleSheet m_AdditionalStyle;

        IGlobalSettings m_GlobalSettings;
        VisualElement m_GlobalSettingsRoot;

        VisualElement m_RootVisualElement;

        [Inject]
        void Setup(IGlobalSettings settings)
        {
            m_GlobalSettings = settings;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            m_RootVisualElement = base.CreateVisualTree(template);
            if (m_AdditionalStyle != null)
            {
                m_RootVisualElement.styleSheets.Add(m_AdditionalStyle);
            }

            return m_RootVisualElement;
        }

        void CreateVisualTree()
        {
            if (m_GlobalSettingsRoot == null || m_GlobalSettings.IsDirty)
            {
                m_GlobalSettingsRoot = m_GlobalSettings.CreateVisualTree();

                m_RootVisualElement.Add(m_GlobalSettingsRoot);
            }

            m_GlobalSettings.RefreshSettingsEnableState();
        }

        public override void OnToolOpened()
        {
            CreateVisualTree();
        }
    }
}
