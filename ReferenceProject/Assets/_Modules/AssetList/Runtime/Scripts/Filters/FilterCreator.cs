using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public interface IFilter
    {
        string Label { get; set; }
        bool IsUsed { get; }
        void FillContent(VisualElement container, IEnumerable<IAsset> assets, AssetSearchFilter searchFilter);
        void ApplyFilter(AssetSearchFilter searchFilter, Chip filterChip);
        void ClearFilter(AssetSearchFilter searchFilter);
        event Action ItemChosen;
    }

    public static class FilterCreator
    {
        public static IFilter Create(AssetListFilterData.FilterTypeData filterTypeData)
        {
            switch (filterTypeData.Type)
            {
                case AssetListFilterData.FilterType.AssetType:
                    return new AssetTypeFilter(filterTypeData.Label);
                case AssetListFilterData.FilterType.CreatedBy:
                    return new CreatedByFilter(filterTypeData.Label);
                case AssetListFilterData.FilterType.CreatedDate:
                    return new CreatedDateFilter(filterTypeData.Label);
                case AssetListFilterData.FilterType.UpdatedBy:
                    return new UpdatedByFilter(filterTypeData.Label);
                case AssetListFilterData.FilterType.UpdatedDate:
                    return new UpdatedDateFilter(filterTypeData.Label);
                case AssetListFilterData.FilterType.Status:
                    return new StatusFilter(filterTypeData.Label);
                default:
                    return null;
            }
        }
    }
}
