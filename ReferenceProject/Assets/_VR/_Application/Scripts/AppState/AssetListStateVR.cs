using System;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.ReferenceProject.AssetList;
using Unity.ReferenceProject.CustomKeyboard;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VR
{
    [Serializable]
    public class AssetListStateVR : AppStateWorldSpacePanel
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [SerializeField]
        AssetListUIController m_AssetListUIController;

        protected override string PanelName => "AssetListPanel";

        protected override Vector2 PanelSize => new Vector2(1366, 768);

        protected override void OnPanelBuilt(UIDocument document)
        {
            var assetDiscoveryVisualElement = m_VisualTreeAsset.Instantiate();
            assetDiscoveryVisualElement.AddToClassList("stretch");
            assetDiscoveryVisualElement[0].AddToClassList("opaque-background");
            assetDiscoveryVisualElement[0].AddToClassList("panel-border-radius");

            var appuiPanel = document.rootVisualElement.Q<Panel>();
            appuiPanel.Add(assetDiscoveryVisualElement);

            m_AssetListUIController.InitUIToolkit(assetDiscoveryVisualElement);
            var keyboardHandler = m_AssetListUIController.GetComponent<KeyboardHandler>();
            if (keyboardHandler != null)
            {
                keyboardHandler.RegisterRootVisualElement(assetDiscoveryVisualElement);
            }

            base.OnPanelBuilt(document);
        }
    }
}