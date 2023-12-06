using System;
using Unity.Cloud.Assets;
using Unity.Properties;
using UnityEngine;
using Unity.ReferenceProject.DataStores;

namespace Unity.ReferenceProject.AssetManager
{
    public class AssetManagerStore : DataStore<AssetManagerViewModel> { }

    [Serializable, GeneratePropertyBag]
    public struct AssetManagerViewModel : IEquatable<AssetManagerViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IAsset Asset { get; set; }

        public bool Equals(AssetManagerViewModel other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == GetType() && Equals((AssetManagerViewModel)obj);
        }

        public override int GetHashCode()
        {
            return Asset.GetHashCode();
        }
    }
}
