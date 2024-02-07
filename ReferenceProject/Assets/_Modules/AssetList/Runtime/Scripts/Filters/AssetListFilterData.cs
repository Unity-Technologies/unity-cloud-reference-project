using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    [CreateAssetMenu(fileName = "AssetListFilterData", menuName = "ReferenceProject/AssetList/FilterData", order = 0)]
    public class AssetListFilterData : ScriptableObject
    {
        public enum FilterType
        {
            AssetType,
            CreatedBy,
            CreatedDate,
            UpdatedBy,
            UpdatedDate,
            Status
        }

        [System.Serializable]
        public class FilterTypeData
        {
            [SerializeField]
            FilterType m_Type;

            [SerializeField]
            string m_Label;

            [SerializeField]
            bool m_IsSelected;

            public FilterType Type => m_Type;
            public string Label => m_Label;
            public bool IsSelected => m_IsSelected;
        }

        [SerializeField]
        FilterTypeData[] m_FilterTypes;

        public FilterTypeData[] FilterTypes => m_FilterTypes;
    }
}
