using System;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.ReferenceProject.AssetManager;
using Unity.ReferenceProject.DataStores;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject
{
    public class AssetNameDisplay : MonoBehaviour
    {
        static readonly string k_NoAsset = "<none>";

        [SerializeField]
        UIDocument m_SourceUIDocument;

        [Header("UXML")]
        [SerializeField]
        string m_HeaderElement = "header";

        Heading m_Header;

        IAssetEvents m_AssetEvents;

        [Inject]
        void Setup(IAssetEvents assetEvents)
        {
            m_AssetEvents = assetEvents;
        }

        void Awake()
        {
            m_Header = m_SourceUIDocument.rootVisualElement.Q<Heading>(m_HeaderElement);
        }

        void OnEnable()
        {
            m_AssetEvents.AssetLoaded += OnAssetLoaded;
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;
        }

        void OnDisable()
        {
            m_AssetEvents.AssetLoaded -= OnAssetLoaded;
            m_AssetEvents.AssetUnloaded -= OnAssetUnloaded;
        }

        void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            SetTitle(asset?.Name);
        }

        void OnAssetUnloaded()
        {
            SetTitle(null);
        }

        void SetTitle(string title)
        {
            m_Header.text = string.IsNullOrEmpty(title) ? k_NoAsset : title;
        }
    }
}
