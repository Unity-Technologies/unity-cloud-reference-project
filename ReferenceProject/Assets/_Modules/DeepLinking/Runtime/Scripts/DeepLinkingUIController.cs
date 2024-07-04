using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using Unity.ReferenceProject.DataStores;
using UnityEngine.UIElements;
using Unity.ReferenceProject.Tools;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.ReferenceProject.AssetManager;
using Zenject;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.ReferenceProject.DeepLinking
{
    public class DeepLinkingUIController : ToolUIController
    {
        [Header("UXML")]
        [SerializeField]
        string m_GenerateURLButtonElement = "generate-url-button";

        [SerializeField]
        string m_URLTextElement = "url-text";

        [SerializeField]
        string m_OpenURLButtonElement = "open-url-button";

        IDeepLinkingController m_DeepLinkingController;
        PropertyValue<IAsset> m_SelectedAsset;
        IAppMessaging m_AppMessaging;
        IClipboard m_Clipboard;

        TextField m_UrlTextField;

        [Inject]
        public void Setup(IDeepLinkingController deepLinkingController, AssetManagerStore assetManagerStore, IAppMessaging appMessaging, IClipboard clipboard)
        {
            m_DeepLinkingController = deepLinkingController;
            m_SelectedAsset = assetManagerStore.GetProperty<IAsset>(nameof(AssetManagerViewModel.Asset));
            m_AppMessaging = appMessaging;
            m_Clipboard = clipboard;
        }

        new void Awake()
        {
            m_DeepLinkingController.LinkConsumptionFailed += OnLinkConsumptionFailed;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_DeepLinkingController.LinkConsumptionFailed -= OnLinkConsumptionFailed;
            m_DeepLinkingController?.Dispose();
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);

            var shareLinkButton = root.Q<ActionButton>(m_GenerateURLButtonElement);
            shareLinkButton.clickable.clicked += OnShareLinkClicked;
            
            shareLinkButton.SetEnabled(m_SelectedAsset.GetValue() != null);

            m_UrlTextField = root.Q<TextField>(m_URLTextElement);

            var openLinkButton = root.Q<ActionButton>(m_OpenURLButtonElement);
            openLinkButton.clickable.clicked += OnOpenUrlClicked;
            openLinkButton.SetEnabled(false);

            m_UrlTextField.RegisterValueChangedCallback(evt =>
            {
                openLinkButton.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue));
            });

            m_SelectedAsset.ValueChanged += asset =>
            {
                shareLinkButton.SetEnabled(asset != null);
            };

            return root;
        }

        async void OnShareLinkClicked()
        {
            try
            {
                var uri = await m_DeepLinkingController.GenerateUri(m_SelectedAsset.GetValue().Descriptor);
                if (m_Clipboard.CopyText(uri.ToString()))
                {
                    m_AppMessaging.ShowSuccess("@DeepLinking:GenerateLinkSuccess");
                }
                else
                {
                    m_AppMessaging.ShowException(new Exception(), "@DeepLinking:GenerateLinkFail");
                }
            }
            catch (Exception ex)
            {
                m_AppMessaging.ShowException(ex, "@DeepLinking:GenerateLinkFail");
            }
        }

        async void OnOpenUrlClicked()
        {
            if (await m_DeepLinkingController.TryConsumeUri(m_UrlTextField.value))
            {
                m_UrlTextField.value = string.Empty;
            }
            else
            {
                m_AppMessaging.ShowError("@DeepLinking:OpenLinkFail", true);
            }
        }
        
        void OnLinkConsumptionFailed(Exception ex)
        {
            m_AppMessaging.ShowException(ex, "@DeepLinking:OpenLinkFail");
        }
    }
}
