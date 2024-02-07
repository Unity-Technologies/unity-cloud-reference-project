using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetGridUIController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_GridItemTemplate;

        [SerializeField]
        ThumbnailPlaceholders m_ThumbnailPlaceholders;

        [SerializeField]
        int m_GridItemSize = 150;

        static readonly string k_AssetGrid = "AssetGrid";
        static readonly string k_ItemRoot = "Root";
        static readonly string k_PlaceholderUssClassName = "image__asset_grid_item_thumbnail--placeholder";

        GridView m_AssetGrid;
        ScrollView m_AssetGridScrollView;
        VisualElement m_WarningMessageNoAssets;
        VisualElement m_WarningMessageNoProjects;
        readonly List<IAsset> m_AssetsListWithoutSearch = new List<IAsset>();

        public event Action<IAsset> AssetSelected;

        public void InitUIToolkit(VisualElement root)
        {
            m_AssetGrid = root.Q<GridView>(k_AssetGrid);
            m_AssetGridScrollView = m_AssetGrid.Q<ScrollView>();
            m_WarningMessageNoAssets = root.Q("AssetWarningMessage");
            m_WarningMessageNoProjects = root.Q("ProjectWarningMessage");
            Utils.SetVisible(m_WarningMessageNoAssets, false);
            Utils.SetVisible(m_WarningMessageNoProjects, false);
            m_AssetGrid.parent.RegisterCallback<GeometryChangedEvent>(OnGridGeometryChanged);

            RefreshGridViewSize();
            m_AssetGrid.itemHeight = (int)m_AssetGrid.itemWidth;
            m_AssetGrid.bindItem = OnBindItem;
            m_AssetGrid.makeItem = () =>
            {
                var item = m_GridItemTemplate.CloneTree();
                return item.Q(k_ItemRoot);
            };
            m_AssetGrid.selectionChanged += OnSelectionChanged;
        }

        public void Add(IAsset asset)
        {
            if (m_AssetGrid.itemsSource == null)
            {
                m_AssetGrid.itemsSource = new List<IAsset>();
            }

            m_AssetsListWithoutSearch.Add(asset);
            var itemsSource = m_AssetGrid.itemsSource as List<IAsset>;
            itemsSource?.Add(asset);
            m_AssetGrid.itemsSource = itemsSource;
            Utils.SetVisible(m_WarningMessageNoAssets, false);
        }

        public void ClearSelection()
        {
            m_AssetGrid.ClearSelection();
        }

        public void Clear()
        {
            m_AssetGrid.ClearSelection();
            m_AssetGrid.itemsSource = null;
            m_AssetsListWithoutSearch.Clear();
            Utils.SetVisible(m_WarningMessageNoAssets, false);
        }

        public void CheckNoAssets()
        {
            Utils.SetVisible(m_WarningMessageNoAssets, m_AssetGrid.itemsSource == null || m_AssetGrid.itemsSource.Count == 0);
        }

        void OnBindItem(VisualElement item, int i)
        {
            RefreshGridItem(item, i);
        }

        static string ItemNameFromIndex(int i)
        {
            return $"GridItem{i}";
        }

        void RefreshGridItem(VisualElement item, int i)
        {
            var asset = (IAsset)m_AssetGrid.itemsSource[i];

            var itemName = ItemNameFromIndex(i);
            item.name = itemName;

            item.Q<Text>("Name").text = asset.Name;
            item.tooltip = asset.Name;

            var badge = item.Q("NotStreamableBadge");
            Utils.SetVisible(badge, false);
            _ = StreamableAssetHelper.IsStreamable(asset, isStreamable => { Utils.SetVisible(badge, !isStreamable); });

            var thumbnail = item.Q<Image>("Thumbnail");
            thumbnail.image = m_ThumbnailPlaceholders.GetDefaultThumbnail(asset.Type);
            thumbnail.AddToClassList(k_PlaceholderUssClassName);

            var progress = item.Q<LinearProgress>("Progress");
            if (asset.PreviewFile != null)
            {
                Utils.SetVisible(progress, true);

                _ = TextureController.GetThumbnail(asset, m_GridItemSize, texture2D =>
                {
                    // Be sure the item still contains that asset
                    if (item.name == itemName)
                    {
                        if (texture2D != null)
                        {
                            thumbnail.image = texture2D;
                            thumbnail.RemoveFromClassList(k_PlaceholderUssClassName);
                        }

                        Utils.SetVisible(progress, false);
                    }
                });
            }
            else
            {
                Utils.SetVisible(progress, false);
            }
        }

        void OnSelectionChanged(IEnumerable<object> selection)
        {
            if (selection?.FirstOrDefault() is IAsset selected)
            {
                AssetSelected?.Invoke(selected);

                // Refreshing item content when user clicks on it to avoid displaying out of date values
                // This is a temporary solution until we have proper event for when an Asset become streamable.
                var index = m_AssetGrid.itemsSource.IndexOf(selected);
                var item = m_AssetGrid.Q(ItemNameFromIndex(index));

                if (item != null)
                {
                    RefreshGridItem(item, index);
                }
            }
        }

        void RefreshGridViewSize()
        {
            var width = m_AssetGridScrollView.contentContainer.resolvedStyle.width;
            var newCountPerRow = Mathf.FloorToInt(width / m_GridItemSize);

            newCountPerRow = Mathf.Max(1, newCountPerRow);

            if (newCountPerRow != m_AssetGrid.columnCount)
            {
                m_AssetGrid.columnCount = newCountPerRow;
                m_AssetGrid.itemHeight = Mathf.RoundToInt(m_AssetGrid.itemWidth);
            }
        }

        void OnGridGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshGridViewSize();
        }

        public void ShowNoProjectsWarning(bool show)
        {
            Utils.SetVisible(m_WarningMessageNoProjects, show);
            if (show)
            {
                Utils.SetVisible(m_WarningMessageNoAssets,false);
            }
        }
        
        public void UpdateWithSearchResult(string arg)
        {
            List<IAsset> assetListSearchResult = new List<IAsset>();
            foreach (var asset in m_AssetsListWithoutSearch)
            {
                if (asset.Name.IndexOf(arg, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    assetListSearchResult.Add(asset);
                }
            }
            m_AssetGrid.itemsSource = assetListSearchResult;
            CheckNoAssets();
        }
    }
}
