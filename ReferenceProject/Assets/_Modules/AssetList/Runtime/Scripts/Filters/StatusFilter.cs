using Unity.Cloud.Assets;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    public class StatusFilter : BaseTextFilter
    {
        public StatusFilter(string label)
            : base(label) { }

        public override void ClearFilter(AssetSearchFilter searchFilter)
        {
            searchFilter.Include().Status.Clear();
        }

        protected override object GetAssetFilteredValue(IAsset asset)
        {
            return asset.Status;
        }

        protected override void ApplySpecificFilter(AssetSearchFilter searchFilter, object value)
        {
            var status = (string)value;
            searchFilter.Include().Status.WithValue(status);
        }

        protected override string GetStringValue(object value)
        {
            return $"{FilterUIController.k_LocalizedAssetList}{value}";
        }
    }
}
