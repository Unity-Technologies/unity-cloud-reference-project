using System;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// A object representing an asset for the AssetTargetField.
    /// </summary>
    [Serializable]
    class AssetReference
    {
        /// <summary>
        /// The name of the asset.
        /// </summary>
        public string name;

        /// <summary>
        /// The type of the asset.
        /// </summary>
        public string type;

        /// <summary>
        /// The type icon of the asset.
        /// </summary>
        public string typeIcon;
    }
}
