using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public class CollectionGridUIController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_GridItemTemplate;

        [SerializeField]
         Vector2 m_GridItemSize = new (200, 50);

        static readonly string k_CollectionGrid = "CollectionGrid";
        static readonly string k_ItemRoot = "Root";

        GridView m_CollectionGrid;
        ScrollView m_CollectionGridScrollView;

        public event Action<IAssetCollection> CollectionSelected;

        public void InitUIToolkit(VisualElement root)
        {
            m_CollectionGrid = root.Q<GridView>(k_CollectionGrid);
            m_CollectionGridScrollView = m_CollectionGrid.Q<ScrollView>();
            m_CollectionGrid.RegisterCallback<GeometryChangedEvent>(OnGridGeometryChanged);

            RefreshGridViewSize();
            m_CollectionGrid.itemHeight = (int)m_GridItemSize.y;
            m_CollectionGrid.bindItem = OnBindItem;
            m_CollectionGrid.makeItem = () =>
            {
                var item = m_GridItemTemplate.CloneTree();
                return item.Q(k_ItemRoot);
            };
            m_CollectionGrid.selectionChanged += OnSelectionChanged;
        }

        public void Populate(List<AssetCollectionInfo> collections)
        {
            m_CollectionGrid.itemsSource = collections;
            RefreshGridViewSize();
        }

        public void Clear()
        {
            m_CollectionGrid.ClearSelection();
            m_CollectionGrid.itemsSource = null;
            m_CollectionGrid.style.flexBasis = 0;
        }

        void OnBindItem(VisualElement item, int i)
        {
            RefreshGridItem(item, i);
        }

        void RefreshGridItem(VisualElement item, int i)
        {
            var collectionInfo = (AssetCollectionInfo)m_CollectionGrid.itemsSource[i];
            var collectionName = item.Q<Text>("Name");
            var assetCount = item.Q<Text>("AssetCount");

            collectionName.text = collectionInfo.Collection.Name;
            item.tooltip = collectionInfo.Collection.Name;

            int count = collectionInfo.AssetCount;
            Common.Utils.SetVisible(assetCount, count > 0);
            assetCount.variables = new object[] { count };
        }

        void OnSelectionChanged(IEnumerable<object> selection)
        {
            if (selection?.FirstOrDefault() is AssetCollectionInfo selected)
            {
                CollectionSelected?.Invoke(selected.Collection);
            }
        }

        void RefreshGridViewSize()
        {
            var width = m_CollectionGridScrollView.contentContainer.resolvedStyle.width;
            var newCountPerRow = Mathf.FloorToInt(width / m_GridItemSize.x);

            newCountPerRow = Mathf.Max(1, newCountPerRow);

            if (newCountPerRow != m_CollectionGrid.columnCount)
            {
                m_CollectionGrid.columnCount = newCountPerRow;
            }

            if (m_CollectionGrid.itemsSource != null)
            {
                var size = m_CollectionGrid.itemsSource.Count;
                if (size > 0)
                {
                    var rowCount = Mathf.CeilToInt(size / (float)m_CollectionGrid.columnCount);
                    m_CollectionGrid.style.flexBasis = rowCount * m_GridItemSize.y;
                    return;
                }
            }
            m_CollectionGrid.style.flexBasis = 0;
        }

        void OnGridGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshGridViewSize();
        }
    }
}
