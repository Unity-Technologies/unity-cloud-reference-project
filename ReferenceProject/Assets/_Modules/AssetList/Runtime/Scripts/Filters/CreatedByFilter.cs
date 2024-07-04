using Unity.Cloud.Assets;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    public class CreatedByFilter : BaseTextFilter
    {
        public CreatedByFilter(string label)
            : base(label) { }

        public override void ClearFilter(AssetSearchFilter searchFilter)
        {
            searchFilter.Include().AuthoringInfo.CreatedBy.Clear();
        }

        protected override object GetAssetFilteredValue(IAsset asset)
        {
            return asset.AuthoringInfo.CreatedBy;
        }

        protected override void ApplySpecificFilter(AssetSearchFilter searchFilter, object value)
        {
            var userId = (string)value;
            searchFilter.Include().AuthoringInfo.CreatedBy.WithValue(userId);
        }

        protected override string GetStringValue(object value)
        {
            // TODO: Get user name from user id (api does not support this yet)
            return (string)value;
        }
    }
}
