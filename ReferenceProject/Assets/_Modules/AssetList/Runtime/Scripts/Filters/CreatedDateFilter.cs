using Unity.Cloud.Assets;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    public class CreatedDateFilter : BaseDateFilter
    {
        public CreatedDateFilter(string label)
            : base(label) { }

        public override void ClearFilter(AssetSearchFilter searchFilter)
        {
            searchFilter.Include().AuthoringInfo.Created.Clear();
        }
    }
}
