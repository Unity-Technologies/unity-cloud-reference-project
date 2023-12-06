using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetInformationContainerUI
    {
        readonly string m_Title;
        readonly VisualElement m_ValueUIElement = new VisualElement();

        static readonly string k_AssetInformationContainerUssClassName = "container__asset-information";
        static readonly string k_ContainerTitleUssClassName = "container__asset-information-title";
        static readonly string k_ContainerValueUssClassName = "container__asset-information-value";
        static readonly string k_EmptyTextUssClassName = "text__asset-information--empty";
        static readonly string k_ChipContainerUssClassName = "container__asset-information-chip";
        static readonly string k_ChipUssClassName = "chip__asset-information";
        static readonly string k_EmptyText = "@AssetList:Empty";
        static readonly string k_ChipOrnamentsUssClassName = "chip__asset-information-ornament";
        static readonly string k_ContainerChipOrnamentUssClassName = "container__asset-information-ornament"; 

        public AssetInformationContainerUI(string title, object value)
        {
            m_Title = title;
            var valueText = new Text();
            
            if (string.IsNullOrEmpty(value?.ToString()))
            {
                valueText.text = k_EmptyText;
                valueText.AddToClassList(k_EmptyTextUssClassName);
            }
            else
            {
                valueText.text = value.ToString();
            }

            m_ValueUIElement = valueText;
        }
        
        public AssetInformationContainerUI(string title, AssetType value)
        { 
            m_Title = title;
            var valueText = new Text();
            if (string.IsNullOrEmpty(value.ToString()))
            {
                valueText.text = k_EmptyText;
                valueText.AddToClassList(k_EmptyTextUssClassName);
                m_ValueUIElement = valueText;
            }
            else{
                valueText.text = "@AssetList:" + value;
                m_ValueUIElement = valueText;
            }
        }
        
        public AssetInformationContainerUI(string title, string value, Texture2D statusCircle)
        {
            m_Title = title;
            var statusChip = new Chip
            {
                label = "@AssetList:"+value,
                ornament = new Image { 
                    image = statusCircle
                }
            };
            statusChip.ornament.AddToClassList(k_ChipOrnamentsUssClassName);
            statusChip.ornament.parent.AddToClassList(k_ContainerChipOrnamentUssClassName);
            AddStyleStatusChip(value, statusChip);
            m_ValueUIElement.Add(statusChip);
        }
        
        public AssetInformationContainerUI(string title, IEnumerable<string> enumerable)
        {
            m_Title = title;
            var valueText = new Text();
            if (title is "@AssetList:Tags" or "@AssetList:SystemTags")
            {
                if (!enumerable.Any())
                {
                    valueText.text = k_EmptyText;
                    valueText.AddToClassList(k_EmptyTextUssClassName);
                    m_ValueUIElement.Add(valueText);
                }
                else
                {
                    var chipContainer = new VisualElement();
                    chipContainer.AddToClassList(k_ChipContainerUssClassName);

                    foreach (var tags in enumerable)
                    {
                        var tag = new Chip
                        {
                            label = tags
                        };
                        tag.AddToClassList(k_ChipUssClassName);
                        chipContainer.Add(tag);
                    }

                    m_ValueUIElement.Add(chipContainer);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(enumerable?.ToString()))
                {
                    valueText.text = k_EmptyText;
                    valueText.AddToClassList(k_EmptyTextUssClassName);
                }
                else
                {
                    valueText.text = string.Join(", ", enumerable);
                }

                m_ValueUIElement = valueText;
            }
        }

        public VisualElement CreateVisualTree()
        {
            var element = new VisualElement();
            element.AddToClassList(k_AssetInformationContainerUssClassName);

            var containerTitle = new VisualElement();
            var title = new Text
            {
                text = m_Title
            };

            containerTitle.AddToClassList(k_ContainerTitleUssClassName);
            containerTitle.Add(title);

            var containerValue = new VisualElement();
            containerValue.AddToClassList(k_ContainerValueUssClassName);

            containerValue.Add(m_ValueUIElement);

            element.Add(containerTitle);
            element.Add(containerValue);

            return element;
        }

        void AddStyleStatusChip(string value, Chip statusChip)
        {
            statusChip.AddToClassList(k_ChipUssClassName);
            
            switch (value)
            {
                case "Published":
                    statusChip.ornament.AddToClassList("chip__tag--published");
                    break;
                case "Draft":
                    statusChip.ornament.AddToClassList("chip__tag--draft");
                    break;
                case "Deleted":
                    statusChip.ornament.AddToClassList("chip__tag--deleted");
                    break;
                case "Withdrawn":
                    statusChip.ornament.AddToClassList("chip__tag--withdrawn");
                    break;
                default:
                    statusChip.ornament.AddToClassList("chip__tag--unknown");
                    break;
            }
        }
    }
}
