using Unity.Cloud.Assets;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    public class UpdatedDateFilter : BaseDateFilter
    {
        public UpdatedDateFilter(string label)
            : base(label) { }

        public override void ClearFilter(AssetSearchFilter searchFilter)
        {
            searchFilter.AuthoringInfo.Updated.Clear();
        }
    }
}
