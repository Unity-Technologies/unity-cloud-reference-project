using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class FilterUIController : MonoBehaviour
    {
        VisualElement m_FilterContainer;
        
        ActionButton m_StreamableButton;

        static readonly string k_StreamableKey = "StreamableFilter";

        public event Action<bool> StreamableFilterChanged;

        public void InitUIToolkit(VisualElement root)
        {
            m_FilterContainer = root.Q<VisualElement>("FilterContainer");
            
            m_StreamableButton = root.Q<ActionButton>("FilterStreamableButton");
            m_StreamableButton.clicked += OnStreamableButtonClicked;

            var filterButton = root.Q<Button>("FilterButton");
            filterButton.clicked += OnFilterButtonClicked;

            // Hide feature until it is implemented
            filterButton.style.display = DisplayStyle.None;
            // ----------------------
            
            var streamableFilter = PlayerPrefs.GetInt(k_StreamableKey, 1);
            if (streamableFilter == 1)
            {
                m_StreamableButton.selected = true;
                StreamableFilterChanged?.Invoke(true);
            }
        }

        static void OnFilterButtonClicked()
        {
            Debug.Log("Filter button clicked");
        }

        void OnStreamableButtonClicked()
        {
            m_StreamableButton.selected = !m_StreamableButton.selected;
            StreamableFilterChanged?.Invoke(m_StreamableButton.selected);
            PlayerPrefs.SetInt(k_StreamableKey, m_StreamableButton.selected ? 1 : 0);
        }
    }
}
