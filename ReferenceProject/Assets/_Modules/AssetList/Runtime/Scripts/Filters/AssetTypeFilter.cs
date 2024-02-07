using Unity.Cloud.Assets;

namespace Unity.ReferenceProject.AssetList
{
    public class AssetTypeFilter : BaseTextFilter
    {
        public AssetTypeFilter(string label)
            : base(label) { }

        public override void ClearFilter(AssetSearchFilter searchFilter)
        {
            searchFilter.Type.Clear();
        }

        protected override object GetAssetFilteredValue(IAsset asset)
        {
            return asset.Type;
        }

        protected override void ApplySpecificFilter(AssetSearchFilter searchFilter, object value)
        {
            var assetType = (AssetType)value;
            searchFilter.Type.Include(assetType);
        }

        protected override string GetStringValue(object value)
        {
            return $"{FilterUIController.k_LocalizedAssetList}{value}";
        }
    }
}
