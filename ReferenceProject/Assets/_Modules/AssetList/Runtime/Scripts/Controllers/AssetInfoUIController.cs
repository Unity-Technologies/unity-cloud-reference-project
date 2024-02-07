using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetInfoUIController : MonoBehaviour
    {
        [SerializeField]
        AssetDisplayedInformation m_AssetDisplayedInformation;

        [SerializeField]
        ThumbnailPlaceholders m_ThumbnailPlaceholders;

        [SerializeField]
        StyleSheet m_StyleSheet;

        Image m_AssetThumbnail;
        VisualElement m_AssetInfoContent;
        Button m_OpenButton;
        Button m_CloseButton;
        Button m_GenerateStreamableButton;
        Heading m_AssetName;
        Drawer m_AssetPanel;
        Chip m_AssetTypeChip;
        Button m_ProjectButton;

        readonly string k_AssetPanel = "AssetPanel";
        readonly string k_OpenButton = "OpenButton";
        readonly string k_CloseButton = "CloseButton";
        readonly string k_GenerateStreamableButton = "GenerateStreamableButton";
        readonly string k_AssetName = "AssetName";
        readonly string k_AssetInfoContent = "AssetInfoContent";
        readonly string k_Thumbnail = "Thumbnail";
        readonly string k_ThumbnailUssClassName = "thumbnail__asset-information";
        readonly string k_AccordionlUssClassName = "accordion__asset-information";
        readonly string k_ProjectInfoButtonUssClassName = "button__asset-information-project";
        readonly string k_ProjectInfoButtonIconUssClassName = "button__asset-information-project--icon";
        readonly string k_ProjectInfoButtonDefaultIconUssClassName = "button__asset-information-project--default-icon";
        readonly string k_LocalizedProject = "@AssetList:Project";
        readonly string k_LocalizedAssetList = "@AssetList:";
        readonly string k_AssetTypeChip = "AssetTypeChip";
        readonly string k_DefaultIcon = "cube";

        public event Action ClosePanelButtonClicked;
        public event Action OpenAssetButtonClicked;
        public event Action GenerateStreamableButtonClicked;
        public event Action<ProjectDescriptor> NeedProjectInfo;

        IAssetRepository m_AssetRepository;

        [Inject]
        void Setup(IAssetRepository assetRepository)
        {
            m_AssetRepository = assetRepository;
        }

        void OnDestroy()
        {
            if (m_AssetPanel != null)
            {
                m_AssetPanel.closed -= OnPanelClosed;
            }

            if (m_OpenButton != null)
            {
                m_OpenButton.clicked -= OnOpenClicked;
            }

            if (m_CloseButton != null)
            {
                m_CloseButton.clicked -= OnClosePanelClicked;
            }

            if (m_GenerateStreamableButton != null)
            {
                m_GenerateStreamableButton.clicked -= OnGenerateStreamableClicked;
            }
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_AssetPanel = root.Q<Drawer>(k_AssetPanel);
            m_OpenButton = root.Q<Button>(k_OpenButton);
            m_AssetName = root.Q<Heading>(k_AssetName);
            m_AssetTypeChip = root.Q<Chip>(k_AssetTypeChip);
            m_CloseButton = root.Q<Button>(k_CloseButton);
            m_GenerateStreamableButton = root.Q<Button>(k_GenerateStreamableButton);
            m_AssetThumbnail = root.Q<Image>(k_Thumbnail);
            m_AssetInfoContent = root.Q<VisualElement>(k_AssetInfoContent);

            m_AssetPanel.closed += OnPanelClosed;
            m_OpenButton.clicked += OnOpenClicked;
            m_CloseButton.clicked += OnClosePanelClicked;
            m_GenerateStreamableButton.clicked += OnGenerateStreamableClicked;
        }

        public void Show(IAsset asset, bool openAccess, bool publishAccess)
        {
            m_AssetPanel.Open();
            m_AssetInfoContent.Clear();
            UpdateAssetHeader(asset);
            UpdateAssetThumbnail(asset);
            UpdateAssetProject(asset);
            UpdateAssetDisplayedInformation(asset);

            m_OpenButton.SetEnabled(false);
            m_GenerateStreamableButton.SetEnabled(publishAccess);

            if (openAccess)
            {
                _ = StreamableAssetHelper.IsStreamable(asset, isStreamable =>
                {
                    m_OpenButton.SetEnabled(isStreamable);
                });
            }
        }

        public void Close()
        {
            m_AssetPanel.Close();
        }

        public void SetOpenButtonState(bool state)
        {
            m_OpenButton.SetEnabled(state);
        }

        public void SetStreamableButtonState(bool state)
        {
            m_GenerateStreamableButton.SetEnabled(state);
        }

        void OnClosePanelClicked()
        {
            Close();
        }

        void OnPanelClosed(Drawer drawer)
        {
            ClosePanelButtonClicked?.Invoke();
        }

        void OnOpenClicked()
        {
            OpenAssetButtonClicked?.Invoke();
        }

        void OnGenerateStreamableClicked()
        {
            GenerateStreamableButtonClicked?.Invoke();
        }

        void UpdateAssetThumbnail(IAsset asset)
        {
            m_AssetThumbnail.Clear();
            m_AssetThumbnail.image = m_ThumbnailPlaceholders.GetDefaultThumbnail(asset.Type);

            if (asset.PreviewFile != null)
            {
                _ = TextureController.GetThumbnail(asset, (int)m_AssetThumbnail.resolvedStyle.width,
                    texture2D => { m_AssetThumbnail.image = texture2D; });
            }

            m_AssetThumbnail.AddToClassList(k_ThumbnailUssClassName);
            m_AssetInfoContent.Add(m_AssetThumbnail);
        }

        void UpdateAssetProject(IAsset asset)
        {
            m_ProjectButton = new Button
            {
                size = Size.S,
                leadingIcon = k_DefaultIcon
            };
            m_ProjectButton.styleSheets.Add(m_StyleSheet);
            m_ProjectButton.AddToClassList(k_ProjectInfoButtonUssClassName);
            NeedProjectInfo?.Invoke(asset.Descriptor.ProjectDescriptor);
            var infoContainer = new AssetInformationContainerUI(k_LocalizedProject, m_ProjectButton);
            m_AssetInfoContent.Add(infoContainer.CreateVisualTree());
        }

        public void SetButtonProjectInfo(string name, string linkUrl, Texture2D texture, Color color)
        {
            m_ProjectButton.title = name;
            m_ProjectButton.tooltip = name;

            m_ProjectButton.clicked += () =>
            {
                Application.OpenURL(linkUrl);
            };

            var projectIcon = m_ProjectButton.Q<Icon>(Button.leadingIconUssClassName);
            if (texture != null)
            {
                projectIcon.image = texture;
                projectIcon.AddToClassList(k_ProjectInfoButtonIconUssClassName);
            }
            else
            {
                projectIcon.AddToClassList(k_ProjectInfoButtonDefaultIconUssClassName);
                projectIcon.style.backgroundColor = color;
            }
        }

        void UpdateAssetDisplayedInformation(IAsset asset)
        {
            var propertyDictionary = m_AssetDisplayedInformation.GetAllInformations();
            foreach (var (propertyName, value) in propertyDictionary)
            {
                if (!value) continue;
                var property = asset.GetType().GetProperty(propertyName);
                if (property == null) continue;

                if (property.PropertyType == typeof(AuthoringInfo))
                {
                    var accordion = CreateAuthoringAccordion(property, asset);

                    m_AssetInfoContent.Add(accordion);
                }
                else
                {
                    var propertyValue = property.GetValue(asset);
                    AssetInformationContainerUI infoContainer;

                    switch (propertyValue)
                    {
                        case IEnumerable<string> enumerable:
                            infoContainer = new AssetInformationContainerUI(k_LocalizedAssetList + propertyName,
                                enumerable);
                            break;
                        case AssetType assetType:
                            infoContainer = new AssetInformationContainerUI(k_LocalizedAssetList, assetType);
                            break;
                        case string stringValue:
                            if (property.Name == "Status")
                            {
                                infoContainer = new AssetInformationContainerUI(k_LocalizedAssetList + propertyName,
                                    stringValue, "status");
                            }
                            else
                            {
                                infoContainer = new AssetInformationContainerUI(k_LocalizedAssetList + propertyName,
                                    stringValue);
                            }

                            break;
                        default:
                            infoContainer = new AssetInformationContainerUI(k_LocalizedAssetList + propertyName,
                                propertyValue);
                            break;
                    }

                    m_AssetInfoContent.Add(infoContainer.CreateVisualTree());
                }
            }
        }

        void UpdateAssetHeader(IAsset asset)
        {
            m_AssetName.text = asset.Name;
            m_AssetTypeChip.label = k_LocalizedAssetList + asset.Type;
        }

        VisualElement CreateAuthoringAccordion(PropertyInfo property, IAsset asset)
        {
            var accordion = new Accordion();
            accordion.AddToClassList(k_AccordionlUssClassName);

            var accordionItem = new AccordionItem
            {
                title = k_LocalizedAssetList + property.Name
            };

            PropertyInfo[] properties = typeof(AuthoringInfo).GetProperties();
            var propertyDictionary = m_AssetDisplayedInformation.GetAuthoringInfo();

            foreach (var propertyAuthoringInfo in properties)
            {
                if (propertyDictionary.TryGetValue(propertyAuthoringInfo.Name, out var value) && value)
                {
                    var info = new AssetInformationContainerUI(k_LocalizedAssetList + propertyAuthoringInfo.Name,
                        propertyAuthoringInfo.GetValue(property.GetValue(asset)));
                    accordionItem.Add(info.CreateVisualTree());
                }
            }

            accordion.Add(accordionItem);
            return accordion;
        }
    }
}
