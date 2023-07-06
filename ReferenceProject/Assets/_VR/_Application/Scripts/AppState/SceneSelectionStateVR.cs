using System;
using Unity.ReferenceProject.ScenesList;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VR
{
    [Serializable]
    public class SceneSelectionStateVR : AppStateWorldSpacePanel
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [SerializeField]
        SceneListUIController m_SceneListUIController;

        protected override string PanelName => "SceneSelectionPanel";

        protected override Vector2 PanelSize => new Vector2(1366, 768);

        protected override void OnPanelBuilt(UIDocument document)
        {
            var sceneListVisualElement = m_VisualTreeAsset.Instantiate();
            sceneListVisualElement.AddToClassList("stretch");
            sceneListVisualElement[0].AddToClassList("opaque-background");
            sceneListVisualElement[0].AddToClassList("panel-border-radius");

            var appuiPanel = document.rootVisualElement.Q<Panel>();
            appuiPanel.Add(sceneListVisualElement);

            m_SceneListUIController.InitUIToolkit(sceneListVisualElement);
            m_SceneListUIController.Refresh();
        }
    }
}