using System;
using System.Collections.Generic;
using Unity.Cloud.Assets;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.ReferenceProject.AssetList
{
    [Serializable]
    public class ThumbnailPlaceholder
    {
        [SerializeField]
        AssetType m_AssetType;

        [SerializeField]
        Texture2D m_Placeholder;

        public AssetType AssetType => m_AssetType;

        public Texture2D Placeholder => m_Placeholder;
    }

    [CreateAssetMenu(fileName = nameof(ThumbnailPlaceholders), menuName = "ReferenceProject/AssetList/" + nameof(ThumbnailPlaceholders))]
    public class ThumbnailPlaceholders : ScriptableObject
    {
        [SerializeField]
        Texture2D m_DefaultPlaceholder;

        [SerializeField]
        List<ThumbnailPlaceholder> m_ThumbnailsPlaceholder = new List<ThumbnailPlaceholder>();

        readonly Dictionary<AssetType, Texture2D> m_TextureByAssetType = new ();

        void OnEnable()
        {
            foreach (var thumbnail in m_ThumbnailsPlaceholder)
            {
                m_TextureByAssetType[thumbnail.AssetType] = thumbnail.Placeholder;
            }
        }

        public List<ThumbnailPlaceholder> ThumbnailsPlaceholder => m_ThumbnailsPlaceholder;

        public Texture2D GetDefaultThumbnail(AssetType assetType)
        {
            if (m_TextureByAssetType.TryGetValue(assetType, out var placeholder))
            {
                return placeholder;
            }

            if(m_DefaultPlaceholder != null)
            {
                return m_DefaultPlaceholder;
            }

            return null;
        }
    }
}
